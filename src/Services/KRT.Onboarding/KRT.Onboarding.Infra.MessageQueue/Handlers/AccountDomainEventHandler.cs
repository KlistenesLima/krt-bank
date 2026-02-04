using MediatR;
using Microsoft.Extensions.Logging;
using KRT.Onboarding.Domain.Events;

namespace KRT.Onboarding.Infra.MessageQueue.Handlers;

public class AccountDomainEventHandler : 
    INotificationHandler<AccountCreatedEvent>,
    INotificationHandler<AccountBlockedEvent>
{
    private readonly ILogger<AccountDomainEventHandler> _logger;

    public AccountDomainEventHandler(ILogger<AccountDomainEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(AccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"[Event] Conta criada: {notification.AccountId} - CPF: {notification.Cpf}");
        return Task.CompletedTask;
    }

    public Task Handle(AccountBlockedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"[Event] Conta bloqueada: {notification.AccountNumber} - Motivo: {notification.Reason}");
        return Task.CompletedTask;
    }
}
