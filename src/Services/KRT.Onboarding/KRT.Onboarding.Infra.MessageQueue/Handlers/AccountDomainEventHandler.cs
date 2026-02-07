using MediatR;
using Microsoft.Extensions.Logging;
using KRT.Onboarding.Domain.Events;
using KRT.BuildingBlocks.MessageBus;
using KRT.BuildingBlocks.MessageBus.Notifications;

namespace KRT.Onboarding.Infra.MessageQueue.Handlers;

public class AccountDomainEventHandler :
    INotificationHandler<AccountCreatedEvent>,
    INotificationHandler<AccountBlockedEvent>,
    INotificationHandler<AccountCreditedEvent>,
    INotificationHandler<AccountDebitedEvent>
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<AccountDomainEventHandler> _logger;

    public AccountDomainEventHandler(IMessageBus messageBus, ILogger<AccountDomainEventHandler> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    public Task Handle(AccountCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Event] Conta criada: {AccountId}", notification.AccountId);

        // Email de boas-vindas
        _messageBus.Publish(new EmailNotification
        {
            To = notification.Email,
            Subject = "Bem-vindo ao KRT Bank!",
            Body = $"Olá {notification.CustomerName}! Sua conta foi criada com sucesso. " +
                   $"Número da conta: {notification.AccountId}.",
            Template = "welcome",
            Priority = 5
        }, "krt.notifications.email", priority: 5);

        // Push notification
        _messageBus.Publish(new PushNotification
        {
            UserId = notification.AccountId,
            Title = "Conta Criada!",
            Body = "Sua conta KRT Bank está ativa. Comece a usar agora!"
        }, "krt.notifications.push");

        return Task.CompletedTask;
    }

    public Task Handle(AccountBlockedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Event] Conta bloqueada: {AccountNumber}", notification.AccountNumber);

        // Email urgente (prioridade alta)
        _messageBus.Publish(new EmailNotification
        {
            To = $"account-{notification.AccountId}@krtbank.com",
            Subject = "⚠️ Sua conta foi bloqueada",
            Body = $"Sua conta {notification.AccountNumber} foi bloqueada. Motivo: {notification.Reason}. " +
                   "Entre em contato com o suporte.",
            Priority = 9
        }, "krt.notifications.email", priority: 9);

        return Task.CompletedTask;
    }

    public Task Handle(AccountCreditedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Event] Conta creditada: {AccountNumber} +{Amount}",
            notification.AccountNumber, notification.Amount);

        // SMS de confirmação
        _messageBus.Publish(new SmsNotification
        {
            PhoneNumber = "+5500000000000",
            Message = $"KRT Bank: Voce recebeu R$ {notification.Amount:N2} na conta {notification.AccountNumber}."
        }, "krt.notifications.sms");

        return Task.CompletedTask;
    }

    public Task Handle(AccountDebitedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[Event] Conta debitada: {AccountNumber} -{Amount}",
            notification.AccountNumber, notification.Amount);

        // Push notification
        _messageBus.Publish(new PushNotification
        {
            UserId = notification.AccountId,
            Title = "Débito realizado",
            Body = $"R$ {notification.Amount:N2} debitados da sua conta."
        }, "krt.notifications.push");

        return Task.CompletedTask;
    }
}
