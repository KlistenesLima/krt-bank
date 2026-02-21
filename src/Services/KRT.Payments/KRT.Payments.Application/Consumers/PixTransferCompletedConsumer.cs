using System.Collections.Generic;
using KRT.BuildingBlocks.EventBus;
using KRT.BuildingBlocks.EventBus.Kafka;
using KRT.BuildingBlocks.MessageBus;
using KRT.BuildingBlocks.MessageBus.Notifications;
using KRT.BuildingBlocks.MessageBus.Receipts;
using KRT.Payments.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using KRT.BuildingBlocks.Infrastructure.Observability;

using System.Diagnostics;
namespace KRT.Payments.Application.Consumers;

/// <summary>
/// Consome eventos Kafka de PIX concluido e dispara tarefas no RabbitMQ.
/// 
/// KAFKA -> RABBITMQ Bridge:
/// - Kafka registra o FATO: "PIX foi concluido" (imutavel, auditavel)
/// - RabbitMQ executa as TAREFAS decorrentes: notificar, gerar comprovante, fazer upload
/// 
/// Este consumer demonstra por que os dois sao indispensaveis:
/// - Kafka garante que o evento nunca se perde (log imutavel, replay possivel)
/// - RabbitMQ garante que cada tarefa e executada exatamente uma vez (ack/nack, DLQ, retry)
/// </summary>
public class PixTransferCompletedConsumer : KafkaConsumerBase<PixTransferCompletedEvent>
{
    public PixTransferCompletedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> settings,
        ILogger<PixTransferCompletedConsumer> logger)
        : base(scopeFactory, settings, logger, "krt.pix.transfer-completed")
    {
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        PixTransferCompletedEvent @event,
        CancellationToken ct)
    {
        var messageBus = serviceProvider.GetRequiredService<IMessageBus>();
        var logger = serviceProvider.GetRequiredService<ILogger<PixTransferCompletedConsumer>>();

        // ═══ OpenTelemetry Metrics ═══
        var sw = Stopwatch.StartNew();

        logger.LogInformation(
            "Processing PIX completed event. TxId={TxId}, Amount={Amount}, Key={Key}",
            @event.TransactionId, @event.Amount, @event.PixKey);

        // 1. EMAIL (RabbitMQ - prioridade normal)
        messageBus.Publish(new EmailNotification
        {
            To = $"account-{@event.SourceAccountId}@krtbank.com",
            Subject = $"PIX de R$ {@event.Amount:N2} realizado com sucesso",
            Body = $"Sua transferencia PIX de R$ {@event.Amount:N2} para a chave " +
                   $"{@event.PixKey} foi concluida em {@event.CompletedAt:dd/MM/yyyy HH:mm}. " +
                   $"ID da transacao: {@event.TransactionId}.",
            Template = "pix-completed"
        }, "krt.notifications.email", priority: 3);

        // 2. SMS (RabbitMQ - confirmacao rapida)
        messageBus.Publish(new SmsNotification
        {
            PhoneNumber = "+5500000000000",
            Message = $"KRT Bank: PIX de R$ {@event.Amount:N2} enviado. " +
                      $"Chave: {MaskPixKey(@event.PixKey)}. " +
                      $"Em {@event.CompletedAt:HH:mm}."
        }, "krt.notifications.sms", priority: 5);

        // 3. PUSH para remetente (RabbitMQ - real-time no app)
        messageBus.Publish(new PushNotification
        {
            UserId = @event.SourceAccountId,
            Title = "PIX enviado",
            Body = $"R$ {@event.Amount:N2} enviado para {MaskPixKey(@event.PixKey)}",
            Action = $"/payments/pix/receipt/{@event.TransactionId}"
        }, "krt.notifications.push", priority: 7);

        // 4. PUSH para destinatario
        messageBus.Publish(new PushNotification
        {
            UserId = @event.DestinationAccountId,
            Title = "PIX recebido!",
            Body = $"Voce recebeu R$ {@event.Amount:N2} via PIX",
            Action = "/statement"
        }, "krt.notifications.push", priority: 7);

        // 5. GERAR COMPROVANTE PDF (RabbitMQ - task queue dedicada)
        messageBus.Publish(new GenerateReceiptMessage
        {
            TransactionId = @event.TransactionId,
            TransactionType = "PIX",
            SourceAccountId = @event.SourceAccountId,
            DestinationAccountId = @event.DestinationAccountId,
            Amount = @event.Amount,
            Currency = @event.Currency,
            PixKey = @event.PixKey,
            CompletedAt = @event.CompletedAt,
            CorrelationId = @event.CorrelationId
        }, "krt.receipts.generate", priority: 3);

        // ═══ KRT Kafka Consumer Metrics ═══
        sw.Stop();
        KrtMetrics.KafkaMessagesConsumed.Add(1, new KeyValuePair<string, object?>("topic", "pix.transfer.completed"));
        KrtMetrics.KafkaConsumerLatency.Record(sw.ElapsedMilliseconds, new KeyValuePair<string, object?>("topic", "pix.transfer.completed"));
        KrtMetrics.PixTransactionsCompleted.Add(1, new KeyValuePair<string, object?>("status", "completed"));
        KrtMetrics.RabbitMqMessagesPublished.Add(1, new KeyValuePair<string, object?>("queue", "krt.receipts.generate"));

        logger.LogInformation(
            "PIX completed fully processed. Dispatched: 1 email, 1 sms, 2 push, 1 receipt. TxId={TxId}, Latency={Latency}ms",
            @event.TransactionId, sw.ElapsedMilliseconds);
    }

    private static string MaskPixKey(string pixKey)
    {
        if (string.IsNullOrEmpty(pixKey) || pixKey.Length < 8) return "****";
        return $"{pixKey[..3]}****{pixKey[^4..]}";
    }
}


