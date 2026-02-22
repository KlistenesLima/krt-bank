using KRT.BuildingBlocks.Infrastructure.Outbox;
using KRT.BuildingBlocks.MessageBus;
using KRT.BuildingBlocks.MessageBus.Notifications;
using KRT.Payments.Application.Events;
using KRT.Payments.Application.Interfaces;
using KRT.Payments.Domain.Enums;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Domain.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace KRT.Payments.Application.Services;

/// <summary>
/// Worker assíncrono que:
/// 1. Busca transações PendingAnalysis
/// 2. Executa scoring de fraude
/// 3. Se aprovado → continua saga (debit/credit)
/// 4. Se rejeitado → marca como falha + notifica
/// 5. Se review → segura pra análise manual
/// </summary>
public class FraudAnalysisWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ITransactionNotifier _notifier;
    private readonly ILogger<FraudAnalysisWorker> _logger;

    public FraudAnalysisWorker(IServiceScopeFactory scopeFactory, ILogger<FraudAnalysisWorker> logger, ITransactionNotifier notifier)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _notifier = notifier;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🛡️ FraudAnalysisWorker started. Polling every 2 seconds.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IPixTransactionRepository>();
                var fraudEngine = scope.ServiceProvider.GetRequiredService<IFraudAnalysisEngine>();
                var onboardingClient = scope.ServiceProvider.GetRequiredService<IOnboardingServiceClient>();
                var outbox = scope.ServiceProvider.GetRequiredService<IOutboxWriter>();
                var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
                var statementService = scope.ServiceProvider.GetRequiredService<IPixStatementService>();

                var pendingTxs = await repository.GetByStatusAsync(
                    PixTransactionStatus.PendingAnalysis, limit: 10, ct: stoppingToken);

                foreach (var tx in pendingTxs)
                {
                    await ProcessTransactionAsync(
                        tx, fraudEngine, onboardingClient, outbox, repository, messageBus, statementService, stoppingToken);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "FraudAnalysisWorker error");
            }

            await Task.Delay(2000, stoppingToken);
        }
    }

    private async Task ProcessTransactionAsync(
        Domain.Entities.PixTransaction tx,
        IFraudAnalysisEngine fraudEngine,
        IOnboardingServiceClient onboardingClient,
        IOutboxWriter outbox,
        IPixTransactionRepository repository,
        IMessageBus messageBus,
        IPixStatementService statementService,
        CancellationToken ct)
    {
        try
        {
            // === STEP 1: Fraud Analysis ===
            var result = await fraudEngine.AnalyzeAsync(tx, ct);

            switch (result.Decision)
            {
                case FraudDecision.Rejected:
                    tx.Reject(result.Score, result.Details);
                    repository.Update(tx);
                    await repository.UnitOfWork.CommitAsync(ct);

                    outbox.Add(OutboxMessage.Create(new FraudAnalysisRejectedEvent(
                        tx.Id, tx.SourceAccountId, tx.DestinationAccountId,
                        tx.Amount, result.Score, result.Details, DateTime.UtcNow), tx.Id.ToString()));
                    await outbox.SaveAsync(ct);

                    // Notificação urgente via RabbitMQ
                    messageBus.Publish(new EmailNotification
                    {
                        To = $"account-{tx.SourceAccountId}@krtbank.com",
                        Subject = "⚠️ Pix Bloqueado — Possível Fraude",
                        Body = $"Sua transferência Pix de R${tx.Amount:N2} foi bloqueada pelo sistema anti-fraude. " +
                               $"Score de risco: {result.Score}/100. Detalhes: {result.Details}. " +
                               "Se você reconhece esta transação, entre em contato com o suporte.",
                        Priority = 9
                    }, "krt.notifications.email", priority: 9);

                    messageBus.Publish(new SmsNotification
                    {
                        PhoneNumber = "+5500000000000",
                        Message = $"KRT Bank ALERTA: Pix R${tx.Amount:N2} BLOQUEADO. Risco: {result.Score}. Contate o suporte."
                    }, "krt.notifications.sms", priority: 9);

                    _logger.LogWarning(
                        "🚫 [Fraud] Pix {TxId} REJEITADO. Score={Score}. {Details}",
                        tx.Id, result.Score, result.Details);
                    break;

                case FraudDecision.UnderReview:
                    tx.HoldForReview(result.Score, result.Details);
                    repository.Update(tx);
                    await repository.UnitOfWork.CommitAsync(ct);

                    messageBus.Publish(new PushNotification
                    {
                        UserId = tx.SourceAccountId,
                        Title = "Pix em Análise",
                        Body = $"Sua transferência de R${tx.Amount:N2} está sendo analisada. " +
                               "Você será notificado em breve."
                    }, "krt.notifications.push");

                    _logger.LogWarning(
                        "⏳ [Fraud] Pix {TxId} em REVISÃO. Score={Score}. {Details}",
                        tx.Id, result.Score, result.Details);
                    break;

                case FraudDecision.Approved:
                    tx.Approve(result.Score, result.Details);
                    repository.Update(tx);
                    await repository.UnitOfWork.CommitAsync(ct);

                    _logger.LogInformation(
                        "✅ [Fraud] Pix {TxId} APROVADO. Score={Score}. Iniciando saga...",
                        tx.Id, result.Score);

                    // === STEP 2: Saga Execution ===
                    await ExecuteSagaAsync(tx, onboardingClient, outbox, repository, messageBus, statementService, ct);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar Pix {TxId}", tx.Id);
            tx.Fail($"Erro interno: {ex.Message}");
            repository.Update(tx);
            await repository.UnitOfWork.CommitAsync(ct);
        }
    }

    private async Task ExecuteSagaAsync(
        Domain.Entities.PixTransaction tx,
        IOnboardingServiceClient onboardingClient,
        IOutboxWriter outbox,
        IPixTransactionRepository repository,
        IMessageBus messageBus,
        IPixStatementService statementService,
        CancellationToken ct)
    {
        // Transita de Approved → Pending (início da saga)
        tx.StartSaga();
        repository.Update(tx);
        await repository.UnitOfWork.CommitAsync(ct);

        outbox.Add(OutboxMessage.Create(new PixTransferInitiatedEvent(
            tx.Id, tx.SourceAccountId, tx.DestinationAccountId,
            tx.Amount, tx.PixKey, DateTime.UtcNow), tx.Id.ToString()));
        await outbox.SaveAsync(ct);

        // === DEBIT ===
        var debitResult = await onboardingClient.DebitAccountAsync(tx.SourceAccountId, tx.Amount, "Pix para " + tx.PixKey);

        if (!debitResult.Success)
        {
            tx.Fail(debitResult.Error ?? "Falha no débito");
            repository.Update(tx);
            await repository.UnitOfWork.CommitAsync(ct);

            outbox.Add(OutboxMessage.Create(new PixTransferFailedEvent(
                tx.Id, tx.SourceAccountId, tx.DestinationAccountId,
                tx.Amount, tx.PixKey, debitResult.Error ?? "Debit failed",
                false, DateTime.UtcNow), tx.Id.ToString()));
            await outbox.SaveAsync(ct);

            _logger.LogWarning("Pix {TxId} debit failed: {Error}", tx.Id, debitResult.Error);
            return;
        }

        tx.MarkSourceDebited();
        repository.Update(tx);
        await repository.UnitOfWork.CommitAsync(ct);

        // === CREDIT ===
        var creditResult = await onboardingClient.CreditAccountAsync(tx.DestinationAccountId, tx.Amount, "Pix de " + tx.PixKey);

        if (!creditResult.Success)
        {
            _logger.LogWarning("Pix {TxId} credit failed. Compensating...", tx.Id);
            var comp = await onboardingClient.CreditAccountAsync(tx.SourceAccountId, tx.Amount, "Estorno Pix " + tx.Id);
            tx.Compensate(creditResult.Error ?? "Falha no crédito");
            repository.Update(tx);
            await repository.UnitOfWork.CommitAsync(ct);

            outbox.Add(OutboxMessage.Create(new PixTransferFailedEvent(
                tx.Id, tx.SourceAccountId, tx.DestinationAccountId,
                tx.Amount, tx.PixKey, creditResult.Error ?? "Credit failed",
                comp.Success, DateTime.UtcNow), tx.Id.ToString()));
            await outbox.SaveAsync(ct);
            return;
        }

        // === COMPLETE ===
        tx.Complete();
        repository.Update(tx);
        await repository.UnitOfWork.CommitAsync(ct);

        // Statement entries for extrato
        try
        {
            await statementService.CreatePixStatementEntriesAsync(
                tx.SourceAccountId, tx.DestinationAccountId,
                tx.Amount, tx.PixKey, tx.Description, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create statement entries for Pix {TxId}", tx.Id);
        }

        outbox.Add(OutboxMessage.Create(new PixTransferCompletedEvent(
            tx.Id, tx.SourceAccountId, tx.DestinationAccountId,
            tx.Amount, tx.PixKey, "BRL", DateTime.UtcNow), tx.Id.ToString()));
        await outbox.SaveAsync(ct);

        // Notificação de sucesso
        messageBus.Publish(new PushNotification
        {
            UserId = tx.SourceAccountId,
            Title = "Pix Enviado!",
            Body = $"R${tx.Amount:N2} enviado com sucesso para {tx.PixKey}."
        }, "krt.notifications.push");

        _logger.LogInformation("✅ Pix {TxId} completed!", tx.Id);
    }
}

