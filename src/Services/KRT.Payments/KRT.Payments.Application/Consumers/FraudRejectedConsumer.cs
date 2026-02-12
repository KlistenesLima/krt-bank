using KRT.BuildingBlocks.EventBus;
using KRT.BuildingBlocks.EventBus.Kafka;
using KRT.BuildingBlocks.MessageBus;
using KRT.BuildingBlocks.MessageBus.Notifications;
using KRT.Payments.Application.Events;
using KRT.Payments.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KRT.Payments.Application.Consumers;

/// <summary>
/// Consome evento Kafka de analise de fraude REJEITADA.
/// 
/// POR QUE KAFKA (e nao so RabbitMQ):
/// - Eventos de fraude sao REGULATORIOS - Banco Central exige rastreabilidade
/// - Kafka mantem log imutavel: quem, quando, por que foi rejeitado
/// - Permite replay para auditorias futuras
/// - Consumer groups: compliance team pode ter seu proprio consumer
/// </summary>
public class FraudRejectedConsumer : KafkaConsumerBase<FraudAnalysisRejectedEvent>
{
    public FraudRejectedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> settings,
        ILogger<FraudRejectedConsumer> logger)
        : base(scopeFactory, settings, logger, "krt.fraud.analysis-rejected")
    {
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        FraudAnalysisRejectedEvent @event,
        CancellationToken ct)
    {
        var repository = serviceProvider.GetRequiredService<IPixTransactionRepository>();
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();
        var messageBus = serviceProvider.GetRequiredService<IMessageBus>();
        var logger = serviceProvider.GetRequiredService<ILogger<FraudRejectedConsumer>>();

        logger.LogWarning(
            "Fraud REJECTED for TxId={TxId}. Score={Score}. Details={Details}",
            @event.TransactionId, @event.FraudScore, @event.Details);

        // 1. Atualizar status no banco
        var transaction = await repository.GetByIdAsync(@event.TransactionId, ct);
        if (transaction != null)
        {
            transaction.Fail($"Fraude detectada - Score: {@event.FraudScore} - {@event.Details}");
            await repository.UnitOfWork.CommitAsync(ct);
        }

        // 2. Publicar evento de falha no KAFKA (audit trail)
        await eventBus.PublishAsync(new PixTransferFailedEvent(
            @event.TransactionId, @event.SourceAccountId, @event.DestinationAccountId,
            @event.Amount, "",
            $"Fraude detectada. Score: {@event.FraudScore}. {@event.Details}",
            WasCompensated: false,
            DateTime.UtcNow
        ), ct);

        // 3. Notificacoes URGENTES via RabbitMQ (prioridade 9)
        messageBus.Publish(new EmailNotification
        {
            To = $"account-{@event.SourceAccountId}@krtbank.com",
            Subject = "Transferencia PIX bloqueada por seguranca",
            Body = $"Sua transferencia PIX de R$ {@event.Amount:N2} foi bloqueada pelo nosso " +
                   $"sistema de seguranca. Se voce nao reconhece esta tentativa, entre em contato " +
                   $"imediatamente com nosso suporte. Protocolo: {@event.TransactionId}.",
            Template = "fraud-blocked",
            Priority = 9
        }, "krt.notifications.email", priority: 9);

        messageBus.Publish(new PushNotification
        {
            UserId = @event.SourceAccountId,
            Title = "PIX bloqueado",
            Body = $"Transferencia de R$ {@event.Amount:N2} bloqueada por seguranca.",
            Action = $"/payments/pix/blocked/{@event.TransactionId}"
        }, "krt.notifications.push", priority: 9);

        messageBus.Publish(new SmsNotification
        {
            PhoneNumber = "+5500000000000",
            Message = $"KRT Bank ALERTA: PIX de R$ {@event.Amount:N2} bloqueado por seguranca. " +
                      $"Protocolo: {@event.TransactionId.ToString()[..8]}. " +
                      "Se nao reconhece, ligue 0800-XXX-XXXX."
        }, "krt.notifications.sms", priority: 9);

        logger.LogWarning(
            "Fraud rejection fully processed. TxId={TxId}. Sent 3 urgent notifications.",
            @event.TransactionId);
    }
}
