using System.Collections.Generic;
using KRT.BuildingBlocks.Infrastructure.Observability;
using static KRT.BuildingBlocks.Infrastructure.Observability.OpenTelemetryExtensions;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using KRT.BuildingBlocks.MessageBus.Notifications;

namespace KRT.BuildingBlocks.MessageBus;

/// <summary>
/// Background service que consome notificações do RabbitMQ.
/// Se falhar 3x, a mensagem vai para a Dead-Letter Queue.
/// </summary>
public class NotificationWorker : BackgroundService
{
    private readonly RabbitMqConnection _connection;
    private readonly ILogger<NotificationWorker> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public NotificationWorker(RabbitMqConnection connection, ILogger<NotificationWorker> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Retry: aguardar RabbitMQ ficar disponivel
        IModel? channel = null;
        for (int attempt = 1; attempt <= 30; attempt++)
        {
            if (stoppingToken.IsCancellationRequested) return;
            try
            {
                channel = _connection.GetChannel();
                _logger.LogInformation("NotificationWorker conectou ao RabbitMQ (tentativa {Attempt})", attempt);
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("NotificationWorker: RabbitMQ indisponivel, tentativa {Attempt}/30: {Error}", attempt, ex.Message);
                if (attempt == 30)
                {
                    _logger.LogError(ex, "NotificationWorker: RabbitMQ indisponivel apos 30 tentativas. Worker desativado.");
                    return;
                }
                await Task.Delay(5000, stoppingToken);
            }
        }
        if (channel == null) return;

        // Processa 1 mensagem por vez (fair dispatch)
        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        // Consumer para emails
        ConsumeQueue(channel, "krt.notifications.email", ProcessEmail, stoppingToken);

        // Consumer para SMS
        ConsumeQueue(channel, "krt.notifications.sms", ProcessSms, stoppingToken);

        // Consumer para push
        ConsumeQueue(channel, "krt.notifications.push", ProcessPush, stoppingToken);

        _logger.LogInformation("NotificationWorker started. Listening on 3 queues.");

        return;
    }

    private void ConsumeQueue(IModel channel, string queueName,
        Action<string, IBasicProperties> handler, CancellationToken ct)
    {
        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (sender, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var retryCount = GetRetryCount(ea.BasicProperties);

            try
            {
                handler(body, ea.BasicProperties);
                channel.BasicAck(ea.DeliveryTag, multiple: false);

                // Métricas OpenTelemetry
                KrtMetrics.RabbitMqMessagesPublished.Add(1, new KeyValuePair<string, object?>("queue", "krt.notifications"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to process message from {Queue} (attempt {Attempt}/3). MessageId={Id}",
                    queueName, retryCount + 1, ea.BasicProperties.MessageId);

                if (retryCount >= 2)
                {
                    // 3 tentativas falharam → vai para DLQ (nack sem requeue)
                    _logger.LogWarning(
                        "Message {Id} sent to Dead-Letter Queue after 3 failures",
                        ea.BasicProperties.MessageId);
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                }
                else
                {
                    // Requeue para tentar novamente
                    channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: true);
                }
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    private void ProcessEmail(string body, IBasicProperties props)
    {
        var email = JsonSerializer.Deserialize<EmailNotification>(body, _jsonOptions);
        if (email == null) throw new InvalidOperationException("Invalid email notification");

        // ═══════════════════════════════════════════════
        // AQUI entra a integração real com SendGrid, SES, etc.
        // Por enquanto, loga simulando o envio
        // ═══════════════════════════════════════════════
        _logger.LogInformation(
            "📧 EMAIL SENT: To={To}, Subject=\"{Subject}\", NotificationId={Id}",
            email.To, email.Subject, email.NotificationId);
    }

    private void ProcessSms(string body, IBasicProperties props)
    {
        var sms = JsonSerializer.Deserialize<SmsNotification>(body, _jsonOptions);
        if (sms == null) throw new InvalidOperationException("Invalid SMS notification");

        _logger.LogInformation(
            "📱 SMS SENT: To={Phone}, Message=\"{Msg}\", NotificationId={Id}",
            sms.PhoneNumber, sms.Message, sms.NotificationId);
    }

    private void ProcessPush(string body, IBasicProperties props)
    {
        var push = JsonSerializer.Deserialize<PushNotification>(body, _jsonOptions);
        if (push == null) throw new InvalidOperationException("Invalid push notification");

        _logger.LogInformation(
            "🔔 PUSH SENT: UserId={UserId}, Title=\"{Title}\", NotificationId={Id}",
            push.UserId, push.Title, push.NotificationId);
    }

    private int GetRetryCount(IBasicProperties props)
    {
        if (props.Headers != null && props.Headers.TryGetValue("x-death", out var death))
        {
            if (death is System.Collections.IList list && list.Count > 0)
                return list.Count;
        }
        return 0;
    }
}



