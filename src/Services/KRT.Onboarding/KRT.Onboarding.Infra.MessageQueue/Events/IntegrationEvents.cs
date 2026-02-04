using KRT.BuildingBlocks.EventBus;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums; // <--- ADICIONADO: Onde vive o AccountType

namespace KRT.Onboarding.Infra.MessageQueue.Events;

// ==================== Account Events ====================

[Topic("krt.accounts.created")]
public record AccountCreatedIntegrationEvent(
    Guid AccountId,
    string AccountNumber,
    Guid CustomerId,
    string CustomerName,
    string CustomerDocument,
    AccountType AccountType,
    DateTime CreatedAt
) : IntegrationEvent;

[Topic("krt.accounts.activated")]
public record AccountActivatedIntegrationEvent(
    Guid AccountId,
    string AccountNumber,
    DateTime ActivatedAt
) : IntegrationEvent;

[Topic("krt.accounts.blocked")]
public record AccountBlockedIntegrationEvent(
    Guid AccountId,
    string AccountNumber,
    string Reason,
    DateTime BlockedAt
) : IntegrationEvent;

[Topic("krt.accounts.unblocked")]
public record AccountUnblockedIntegrationEvent(
    Guid AccountId,
    string AccountNumber,
    DateTime UnblockedAt
) : IntegrationEvent;

[Topic("krt.accounts.closed")]
public record AccountClosedIntegrationEvent(
    Guid AccountId,
    string AccountNumber,
    string Reason,
    DateTime ClosedAt
) : IntegrationEvent;

// ==================== Transaction Events ====================

[Topic("krt.accounts.debited")]
public record AccountDebitedIntegrationEvent(
    Guid AccountId,
    string AccountNumber,
    Guid TransactionId,
    decimal Amount,
    string Currency,
    decimal NewBalance,
    string Description,
    Guid? ReferenceId,
    DateTime OccurredAt
) : IntegrationEvent;

[Topic("krt.accounts.credited")]
public record AccountCreditedIntegrationEvent(
    Guid AccountId,
    string AccountNumber,
    Guid TransactionId,
    decimal Amount,
    string Currency,
    decimal NewBalance,
    string Description,
    Guid? ReferenceId,
    DateTime OccurredAt
) : IntegrationEvent;

// ==================== Balance Events ====================

[Topic("krt.accounts.balance-reserved")]
public record BalanceReservedIntegrationEvent(
    Guid AccountId,
    string AccountNumber,
    decimal Amount,
    string Reason,
    DateTime ReservedAt
) : IntegrationEvent;

[Topic("krt.accounts.balance-released")]
public record BalanceReleasedIntegrationEvent(
    Guid AccountId,
    string AccountNumber,
    decimal Amount,
    string Reason,
    DateTime ReleasedAt
) : IntegrationEvent;
