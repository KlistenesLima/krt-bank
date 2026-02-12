using System.Collections.Generic;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using KRT.BuildingBlocks.MessageBus.Receipts;
using KRT.BuildingBlocks.MessageBus.Storage;
using KRT.BuildingBlocks.Infrastructure.Observability;
using static KRT.BuildingBlocks.Infrastructure.Observability.OpenTelemetryExtensions;

namespace KRT.BuildingBlocks.MessageBus;

/// <summary>
/// Background service que consome tarefas de geração de comprovantes do RabbitMQ.
/// 
/// Pipeline de 2 estágios:
///   Estágio 1: krt.receipts.generate → Gera PDF → Publica krt.receipts.upload
///   Estágio 2: krt.receipts.upload → Faz upload para Backblaze B2 via S3-compatible API
/// </summary>
public class ReceiptWorker : BackgroundService
{
    private readonly RabbitMqConnection _connection;
    private readonly IMessageBus _messageBus;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReceiptWorker> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ReceiptWorker(
        RabbitMqConnection connection,
        IMessageBus messageBus,
        IServiceScopeFactory scopeFactory,
        ILogger<ReceiptWorker> logger)
    {
        _connection = connection;
        _messageBus = messageBus;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IModel? channel = null;
        for (int attempt = 1; attempt <= 30; attempt++)
        {
            if (stoppingToken.IsCancellationRequested) return;
            try
            {
                channel = _connection.GetChannel();
                _logger.LogInformation("ReceiptWorker conectou ao RabbitMQ (tentativa {Attempt})", attempt);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("ReceiptWorker: RabbitMQ indisponivel, tentativa {Attempt}/30: {Error}", attempt, ex.Message);
                if (attempt == 30) { _logger.LogError(ex, "ReceiptWorker desativado."); return; }
                await Task.Delay(5000, stoppingToken);
            }
        }
        if (channel == null) return;

        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        ConsumeGenerateQueue(channel);
        ConsumeUploadQueue(channel);
        _logger.LogInformation("ReceiptWorker started. Listening on 2 queues: generate + upload.");
    }

    private void ConsumeGenerateQueue(IModel channel)
    {
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var retryCount = GetRetryCount(ea.BasicProperties);
            try
            {
                var message = JsonSerializer.Deserialize<GenerateReceiptMessage>(body, _jsonOptions)
                    ?? throw new InvalidOperationException("Invalid receipt message");

                _logger.LogInformation("Generating PDF for TxId={TxId}, Amount={Amount}",
                    message.TransactionId, message.Amount);

                var pdfContent = GeneratePixReceiptPdf(message);
                var fileName = $"pix/{message.CompletedAt:yyyy/MM/dd}/{message.TransactionId}.pdf";

                _logger.LogInformation("PDF generated for TxId={TxId}. Size={Size} bytes.",
                    message.TransactionId, pdfContent.Length);

                // Enfileirar upload no próximo estágio
                _messageBus.Publish(new UploadReceiptMessage
                {
                    ReceiptId = message.ReceiptId,
                    TransactionId = message.TransactionId,
                    TransactionType = message.TransactionType,
                    FileName = fileName,
                    PdfContent = pdfContent,
                    GeneratedAt = DateTime.UtcNow,
                    CorrelationId = message.CorrelationId
                }, "krt.receipts.upload", priority: 3);

                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate receipt (attempt {Attempt}/3)", retryCount + 1);
                channel.BasicNack(ea.DeliveryTag, false, requeue: retryCount < 2);
            }
        };
        channel.BasicConsume(queue: "krt.receipts.generate", autoAck: false, consumer: consumer);
    }

    private void ConsumeUploadQueue(IModel channel)
    {
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (sender, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var retryCount = GetRetryCount(ea.BasicProperties);
            try
            {
                var message = JsonSerializer.Deserialize<UploadReceiptMessage>(body, _jsonOptions)
                    ?? throw new InvalidOperationException("Invalid upload message");

                _logger.LogInformation(
                    "Uploading receipt to Backblaze B2. TxId={TxId}, File={File}, Size={Size}bytes",
                    message.TransactionId, message.FileName, message.PdfContent.Length);

                // ═══════════════════════════════════════════════════
                // UPLOAD REAL para Backblaze B2 via S3-compatible API
                // ═══════════════════════════════════════════════════
                using var scope = _scopeFactory.CreateScope();
                var cloudStorage = scope.ServiceProvider.GetRequiredService<ICloudStorage>();

                var result = await cloudStorage.UploadAsync(
                    message.FileName,
                    message.PdfContent,
                    message.ContentType);

                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"B2 upload failed for {message.FileName}: {result.Error}");
                }

                _logger.LogInformation(
                    "RECEIPT UPLOADED to B2: TxId={TxId}, File={File}, Size={Size}bytes, ETag={ETag}, Url={Url}",
                    message.TransactionId, result.FileName, result.SizeBytes, result.ETag, result.Url);

                KrtMetrics.B2UploadsCompleted.Add(1);
                KrtMetrics.RabbitMqMessagesPublished.Add(1, new KeyValuePair<string, object?>("queue", "krt.receipts.upload"));
                channel.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload receipt (attempt {Attempt}/3)"
                );
                KrtMetrics.B2UploadsFailed.Add(1);
                _logger.LogError(ex,
                    "B2 upload metric recorded for failed attempt", retryCount + 1);
                channel.BasicNack(ea.DeliveryTag, false, requeue: retryCount < 2);
            }
        };
        channel.BasicConsume(queue: "krt.receipts.upload", autoAck: false, consumer: consumer);
    }

        /// <summary>
    /// Gera PDF profissional do comprovante PIX usando QuestPDF.
    /// Design inspirado em comprovantes de bancos digitais brasileiros.
    /// </summary>
    private byte[] GeneratePixReceiptPdf(GenerateReceiptMessage msg)
    {
        // Licença Community (gratuita para receita < $1M/ano)
        QuestPDF.Settings.License = LicenseType.Community;

        var document = new PixReceiptDocument(msg);
        return document.GeneratePdf();
    }

    private int GetRetryCount(IBasicProperties props)
    {
        if (props.Headers != null && props.Headers.TryGetValue("x-death", out var death))
            if (death is System.Collections.IList list && list.Count > 0)
                return list.Count;
        return 0;
    }
}



