using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Enums;

namespace KRT.Payments.Domain.Entities;

/// <summary>
/// Aggregate Root representando uma transacao Pix completa.
/// Implementa State Machine para controle da Saga.
/// </summary>
public class PixTransaction : Entity, IAggregateRoot
{
    public Guid SourceAccountId { get; private set; }
    public Guid DestinationAccountId { get; private set; }
    public string PixKey { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public PixTransactionStatus Status { get; private set; }
    public string? Description { get; private set; }
    public string? FailureReason { get; private set; }
    public Guid IdempotencyKey { get; private set; }

    // Saga tracking
    public bool SourceDebited { get; private set; }
    public bool DestinationCredited { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    protected PixTransaction() { } // EF Core

    public PixTransaction(
        Guid sourceAccountId,
        Guid destinationAccountId,
        string pixKey,
        decimal amount,
        string? description,
        Guid idempotencyKey)
    {
        Id = Guid.NewGuid();
        SourceAccountId = sourceAccountId;
        DestinationAccountId = destinationAccountId;
        PixKey = pixKey;
        Amount = amount;
        Description = description;
        IdempotencyKey = idempotencyKey;
        Status = PixTransactionStatus.Pending;
        SourceDebited = false;
        DestinationCredited = false;
    }

    // === STATE MACHINE ===

    public void MarkSourceDebited()
    {
        if (Status != PixTransactionStatus.Pending)
            throw new InvalidOperationException("Apenas transacoes Pending podem ser debitadas");
        SourceDebited = true;
        Status = PixTransactionStatus.SourceDebited;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkDestinationCredited()
    {
        if (Status != PixTransactionStatus.SourceDebited)
            throw new InvalidOperationException("Source deve ser debitado antes do credito");
        DestinationCredited = true;
        Status = PixTransactionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkFailed(string reason)
    {
        FailureReason = reason;
        Status = PixTransactionStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkCompensated()
    {
        Status = PixTransactionStatus.Compensated;
        SourceDebited = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
