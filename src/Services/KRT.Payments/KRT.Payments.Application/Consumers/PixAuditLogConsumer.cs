using KRT.BuildingBlocks.EventBus;
using KRT.BuildingBlocks.EventBus.Kafka;
using KRT.Payments.Application.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KRT.Payments.Application.Consumers;

/// <summary>
/// Consumer Kafka para AUDIT LOG de todas as transacoes PIX.
/// 
/// POR QUE ISSO SO E POSSIVEL COM KAFKA (e nao RabbitMQ):
/// 
/// 1. LOG IMUTAVEL: Kafka retem mensagens por tempo configuravel (ex: 7 anos).
///    RabbitMQ deleta apos consumo.
/// 
/// 2. CONSUMER GROUPS INDEPENDENTES: Este consumer roda no group "krt-audit-group",
///    separado do "krt-payments-group". Cada group recebe TODAS as mensagens.
///    No RabbitMQ, uma mensagem so vai para UM consumer por fila.
/// 
/// 3. REPLAY: Podemos resetar o offset e reprocessar TODOS os eventos.
/// 
/// 4. COMPLIANCE BACEN: Banco Central exige rastreabilidade completa de
///    transacoes PIX por no minimo 5 anos.
/// </summary>
public class PixAuditLogConsumer : KafkaConsumerBase<PixTransferCompletedEvent>
{
    public PixAuditLogConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> settings,
        ILogger<PixAuditLogConsumer> logger)
        : base(scopeFactory, settings, logger, "krt.pix.transfer-completed")
    {
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        PixTransferCompletedEvent @event,
        CancellationToken ct)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<PixAuditLogConsumer>>();

        // AUDIT LOG ESTRUTURADO - Serilog+Seq captura para consulta
        logger.LogInformation(
            "[AUDIT] PIX_COMPLETED | TxId={TransactionId} | From={SourceAccountId} | " +
            "To={DestinationAccountId} | Amount={Amount} | Currency={Currency} | " +
            "PixKey={PixKey} | CompletedAt={CompletedAt:O} | EventId={EventId} | " +
            "CorrelationId={CorrelationId} | Source={Source} | Version={Version}",
            @event.TransactionId, @event.SourceAccountId, @event.DestinationAccountId,
            @event.Amount, @event.Currency, @event.PixKey, @event.CompletedAt,
            @event.Id, @event.CorrelationId, @event.Source, @event.Version);

        await Task.CompletedTask;
    }
}

/// <summary>
/// Audit consumer para transacoes PIX que falharam.
/// Registra fraudes, falhas de saga, erros de compensacao.
/// </summary>
public class PixFailedAuditLogConsumer : KafkaConsumerBase<PixTransferFailedEvent>
{
    public PixFailedAuditLogConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> settings,
        ILogger<PixFailedAuditLogConsumer> logger)
        : base(scopeFactory, settings, logger, "krt.pix.transfer-failed")
    {
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        PixTransferFailedEvent @event,
        CancellationToken ct)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<PixFailedAuditLogConsumer>>();

        logger.LogWarning(
            "[AUDIT] PIX_FAILED | TxId={TransactionId} | From={SourceAccountId} | " +
            "To={DestinationAccountId} | Amount={Amount} | Reason={FailureReason} | " +
            "WasCompensated={WasCompensated} | FailedAt={FailedAt:O} | " +
            "EventId={EventId} | CorrelationId={CorrelationId}",
            @event.TransactionId, @event.SourceAccountId, @event.DestinationAccountId,
            @event.Amount, @event.FailureReason, @event.WasCompensated,
            @event.FailedAt, @event.Id, @event.CorrelationId);

        await Task.CompletedTask;
    }
}
