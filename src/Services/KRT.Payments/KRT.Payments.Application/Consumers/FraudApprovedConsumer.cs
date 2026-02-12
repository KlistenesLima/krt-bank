using System.Net.Http.Json;
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
/// Consome evento Kafka de analise de fraude APROVADA.
/// 
/// Executa o Saga Pattern (Orchestration):
///   1. Debita conta origem (via HTTP para Onboarding)
///   2. Credita conta destino (via HTTP para Onboarding)
///   3. Se debito OK + credito OK -> publica PixTransferCompleted no Kafka
///   4. Se credito falhar -> compensa (estorna debito) -> publica PixTransferFailed
/// 
/// POR QUE KAFKA AQUI:
/// - O evento de fraude aprovada e um FATO imutavel no log
/// - Se este consumer falhar, o Kafka permite REPLAY do evento
/// - O offset so avanca apos processamento completo (at-least-once)
/// </summary>
public class FraudApprovedConsumer : KafkaConsumerBase<FraudAnalysisApprovedEvent>
{
    public FraudApprovedConsumer(
        IServiceScopeFactory scopeFactory,
        IOptions<KafkaSettings> settings,
        ILogger<FraudApprovedConsumer> logger)
        : base(scopeFactory, settings, logger, "krt.fraud.analysis-approved")
    {
    }

    protected override async Task HandleEventAsync(
        IServiceProvider serviceProvider,
        FraudAnalysisApprovedEvent @event,
        CancellationToken ct)
    {
        var repository = serviceProvider.GetRequiredService<IPixTransactionRepository>();
        var eventBus = serviceProvider.GetRequiredService<IEventBus>();
        var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
        var logger = serviceProvider.GetRequiredService<ILogger<FraudApprovedConsumer>>();

        logger.LogInformation(
            "Fraud APPROVED for TxId={TxId}. Score={Score}. Starting Saga...",
            @event.TransactionId, @event.FraudScore);

        var transaction = await repository.GetByIdAsync(@event.TransactionId, ct);
        if (transaction == null)
        {
            logger.LogWarning("Transaction {TxId} not found. Skipping.", @event.TransactionId);
            return;
        }

        var client = httpClientFactory.CreateClient("OnboardingApi");

        // SAGA STEP 1: Debitar conta origem
        var debitResult = await ExecuteStepAsync(client, "api/accounts/debit", new
        {
            AccountId = @event.SourceAccountId,
            Amount = @event.Amount,
            TransactionId = @event.TransactionId,
            Description = $"PIX enviado - TxId:{@event.TransactionId}"
        }, logger, ct);

        if (!debitResult)
        {
            logger.LogWarning("Debit FAILED for TxId={TxId}. Saga aborted.", @event.TransactionId);
            transaction.Fail("Falha ao debitar conta origem");
            await repository.UnitOfWork.CommitAsync(ct);
            await PublishFailedEvent(eventBus, @event, "Saldo insuficiente ou conta indisponivel", false, ct);
            return;
        }

        // SAGA STEP 2: Creditar conta destino
        var creditResult = await ExecuteStepAsync(client, "api/accounts/credit", new
        {
            AccountId = @event.DestinationAccountId,
            Amount = @event.Amount,
            TransactionId = @event.TransactionId,
            Description = $"PIX recebido - TxId:{@event.TransactionId}"
        }, logger, ct);

        if (!creditResult)
        {
            // SAGA COMPENSATION: Estornar debito
            logger.LogWarning("Credit FAILED for TxId={TxId}. Compensating...", @event.TransactionId);

            await ExecuteStepAsync(client, "api/accounts/credit", new
            {
                AccountId = @event.SourceAccountId,
                Amount = @event.Amount,
                TransactionId = @event.TransactionId,
                Description = $"ESTORNO PIX - Compensacao Saga - TxId:{@event.TransactionId}"
            }, logger, ct);

            transaction.Fail("Falha ao creditar conta destino - debito estornado");
            await repository.UnitOfWork.CommitAsync(ct);
            await PublishFailedEvent(eventBus, @event, "Conta destino indisponivel", true, ct);
            return;
        }

        // SAGA SUCCESS
        transaction.Complete();
        await repository.UnitOfWork.CommitAsync(ct);

        await eventBus.PublishAsync(new PixTransferCompletedEvent(
            @event.TransactionId,
            @event.SourceAccountId,
            @event.DestinationAccountId,
            @event.Amount,
            "",
            "BRL",
            DateTime.UtcNow
        ), ct);

        logger.LogInformation(
            "Saga COMPLETED for TxId={TxId}. Amount={Amount}.",
            @event.TransactionId, @event.Amount);
    }

    private static async Task<bool> ExecuteStepAsync(
        HttpClient client, string endpoint, object payload, ILogger logger, CancellationToken ct)
    {
        try
        {
            var response = await client.PostAsJsonAsync(endpoint, payload, ct);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP error calling {Endpoint}", endpoint);
            return false;
        }
    }

    private static async Task PublishFailedEvent(
        IEventBus eventBus, FraudAnalysisApprovedEvent @event,
        string reason, bool wasCompensated, CancellationToken ct)
    {
        await eventBus.PublishAsync(new PixTransferFailedEvent(
            @event.TransactionId, @event.SourceAccountId, @event.DestinationAccountId,
            @event.Amount, "", reason, wasCompensated, DateTime.UtcNow
        ), ct);
    }
}

