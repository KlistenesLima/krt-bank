using KRT.BuildingBlocks.EventBus;

namespace KRT.Payments.Application.Events;

[Topic("krt.pix.transfer-completed")]
public record PixTransferCompletedEvent(
    Guid TransactionId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string PixKey,
    string Currency,
    DateTime CompletedAt
) : IntegrationEvent;

[Topic("krt.pix.transfer-failed")]
public record PixTransferFailedEvent(
    Guid TransactionId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string PixKey,
    string FailureReason,
    bool WasCompensated,
    DateTime FailedAt
) : IntegrationEvent;

[Topic("krt.pix.transfer-initiated")]
public record PixTransferInitiatedEvent(
    Guid TransactionId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string PixKey,
    DateTime InitiatedAt
) : IntegrationEvent;
