using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Enums;

namespace KRT.Payments.Domain.Entities;

/// <summary>
/// Aggregate Root para transação Pix com State Machine da Saga.
/// Estados: Pending → SourceDebited → Completed | Failed | Compensated
/// </summary>
public class PixTransaction : AggregateRoot
{
    public Guid SourceAccountId { get; private set; }
    public Guid DestinationAccountId { get; private set; }
    public string PixKey { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public string? Description { get; private set; }
    public PixTransactionStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public Guid IdempotencyKey { get; private set; }

    // Saga Flags
    public bool SourceDebited { get; private set; }
    public bool DestinationCredited { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    protected PixTransaction() { } // EF Core

    public PixTransaction(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string pixKey,
        string? description = null,
        Guid? idempotencyKey = null)
    {
        Id = Guid.NewGuid();
        SourceAccountId = sourceAccountId;
        DestinationAccountId = destinationAccountId;
        Amount = amount;
        PixKey = pixKey;
        Description = description ?? string.Empty;
        Status = PixTransactionStatus.Pending;
        IdempotencyKey = idempotencyKey ?? Guid.NewGuid();
        SourceDebited = false;
        DestinationCredited = false;
        CreatedAt = DateTime.UtcNow;
    }

    // === STATE MACHINE ===

    public void MarkSourceDebited()
    {
        if (Status != PixTransactionStatus.Pending)
            throw new InvalidOperationException($"Cannot debit source from status {Status}");
        Status = PixTransactionStatus.SourceDebited;
        SourceDebited = true;
    }

    public void MarkDestinationCredited()
    {
        if (Status != PixTransactionStatus.SourceDebited)
            throw new InvalidOperationException($"Cannot credit destination from status {Status}");
        Status = PixTransactionStatus.Completed;
        DestinationCredited = true;
        CompletedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        Status = PixTransactionStatus.Failed;
        FailureReason = reason;
    }

    public void MarkCompensated()
    {
        Status = PixTransactionStatus.Compensated;
        SourceDebited = false;
    }
}
