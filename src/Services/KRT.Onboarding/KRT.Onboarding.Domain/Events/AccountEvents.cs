using KRT.BuildingBlocks.Domain;

namespace KRT.Onboarding.Domain.Events;

public record AccountCreatedEvent(Guid AccountId, string CustomerName, string Cpf, string Email, string AccountNumber) : DomainEvent;

public record AccountActivatedEvent(Guid AccountId, string AccountNumber) : DomainEvent;

public record AccountBlockedEvent(Guid AccountId, string AccountNumber, string Reason) : DomainEvent;

public record AccountUnblockedEvent(Guid AccountId, string AccountNumber) : DomainEvent;

public record AccountClosedEvent(Guid AccountId, string AccountNumber, string Reason) : DomainEvent;

public record AccountDebitedEvent(Guid AccountId, string AccountNumber, decimal Amount, Guid TransactionId) : DomainEvent;

public record AccountCreditedEvent(Guid AccountId, string AccountNumber, decimal Amount, Guid TransactionId) : DomainEvent;

public record BalanceReservedEvent(Guid AccountId, string AccountNumber, decimal Amount, string Reason) : DomainEvent;

public record BalanceReleasedEvent(Guid AccountId, string AccountNumber, decimal Amount, string Reason) : DomainEvent;
