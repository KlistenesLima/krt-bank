using MediatR;
using Microsoft.Extensions.Logging;
using KRT.Onboarding.Domain.Events;
using KRT.Onboarding.Infra.Data.Context;
using KRT.Onboarding.Infra.MessageQueue.Events;
using KRT.BuildingBlocks.Infrastructure.Outbox;

namespace KRT.Onboarding.Infra.MessageQueue.Handlers;

public class AccountDomainEventHandler :
    INotificationHandler<AccountCreatedEvent>,
    INotificationHandler<AccountActivatedEvent>,
    INotificationHandler<AccountBlockedEvent>,
    INotificationHandler<AccountClosedEvent>,
    INotificationHandler<AccountDebitedEvent>,
    INotificationHandler<AccountCreditedEvent>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<AccountDomainEventHandler> _logger;

    public AccountDomainEventHandler(ApplicationDbContext dbContext, ILogger<AccountDomainEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(AccountCreatedEvent e, CancellationToken ct)
    {
        _logger.LogInformation("[Event] Account created: {AccountId}", e.AccountId);
        _dbContext.OutboxMessages.Add(OutboxMessage.Create(
            new AccountCreatedIntegrationEvent(
                e.AccountId, e.AccountNumber, e.AccountId, e.CustomerName,
                e.Cpf, Domain.Enums.AccountType.Checking, DateTime.UtcNow),
            e.EventId.ToString()));
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Handle(AccountActivatedEvent e, CancellationToken ct)
    {
        _logger.LogInformation("[Event] Account activated: {AccountId}", e.AccountId);
        _dbContext.OutboxMessages.Add(OutboxMessage.Create(
            new AccountActivatedIntegrationEvent(e.AccountId, e.AccountNumber, DateTime.UtcNow),
            e.EventId.ToString()));
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Handle(AccountBlockedEvent e, CancellationToken ct)
    {
        _logger.LogInformation("[Event] Account blocked: {AccountId} - {Reason}", e.AccountId, e.Reason);
        _dbContext.OutboxMessages.Add(OutboxMessage.Create(
            new AccountBlockedIntegrationEvent(e.AccountId, e.AccountNumber, e.Reason, DateTime.UtcNow),
            e.EventId.ToString()));
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Handle(AccountClosedEvent e, CancellationToken ct)
    {
        _logger.LogInformation("[Event] Account closed: {AccountId}", e.AccountId);
        _dbContext.OutboxMessages.Add(OutboxMessage.Create(
            new AccountClosedIntegrationEvent(e.AccountId, e.AccountNumber, e.Reason, DateTime.UtcNow),
            e.EventId.ToString()));
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Handle(AccountDebitedEvent e, CancellationToken ct)
    {
        _logger.LogInformation("[Event] Account debited: {AccountId} Amount: {Amount}", e.AccountId, e.Amount);
        _dbContext.OutboxMessages.Add(OutboxMessage.Create(
            new AccountDebitedIntegrationEvent(
                e.AccountId, e.AccountNumber, e.TransactionId, e.Amount,
                "BRL", 0m, "Pix Debit", null, DateTime.UtcNow),
            e.EventId.ToString()));
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Handle(AccountCreditedEvent e, CancellationToken ct)
    {
        _logger.LogInformation("[Event] Account credited: {AccountId} Amount: {Amount}", e.AccountId, e.Amount);
        _dbContext.OutboxMessages.Add(OutboxMessage.Create(
            new AccountCreditedIntegrationEvent(
                e.AccountId, e.AccountNumber, e.TransactionId, e.Amount,
                "BRL", 0m, "Pix Credit", null, DateTime.UtcNow),
            e.EventId.ToString()));
        await _dbContext.SaveChangesAsync(ct);
    }
}
