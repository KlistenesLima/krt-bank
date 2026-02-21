using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Enums;

namespace KRT.Payments.Domain.Entities;

/// <summary>
/// Aggregate Root representando uma transação Pix completa.
/// Implementa State Machine para controle da Saga + Fraud Analysis.
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

    // Fraud analysis
    public int? FraudScore { get; private set; }
    public string? FraudDetails { get; private set; }
    public DateTime? FraudAnalyzedAt { get; private set; }

    protected PixTransaction() { } // EF Core

    public PixTransaction(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string pixKey,
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
        Status = PixTransactionStatus.PendingAnalysis;  // Inicia em análise
        SourceDebited = false;
        DestinationCredited = false;
    }

    // === FRAUD ANALYSIS STATE MACHINE ===

    public void Approve(int score, string details)
    {
        if (Status != PixTransactionStatus.PendingAnalysis)
            throw new InvalidOperationException("Apenas transações PendingAnalysis podem ser aprovadas");
        FraudScore = score;
        FraudDetails = details;
        FraudAnalyzedAt = DateTime.UtcNow;
        Status = PixTransactionStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(int score, string details)
    {
        if (Status != PixTransactionStatus.PendingAnalysis && Status != PixTransactionStatus.UnderReview)
            throw new InvalidOperationException("Apenas transações em análise podem ser rejeitadas");
        FraudScore = score;
        FraudDetails = details;
        FraudAnalyzedAt = DateTime.UtcNow;
        Status = PixTransactionStatus.Rejected;
        FailureReason = $"Fraude detectada (score: {score}). {details}";
        UpdatedAt = DateTime.UtcNow;
    }

    public void HoldForReview(int score, string details)
    {
        if (Status != PixTransactionStatus.PendingAnalysis)
            throw new InvalidOperationException("Apenas transações PendingAnalysis podem ir para revisão");
        FraudScore = score;
        FraudDetails = details;
        FraudAnalyzedAt = DateTime.UtcNow;
        Status = PixTransactionStatus.UnderReview;
        UpdatedAt = DateTime.UtcNow;
    }

    // === SAGA STATE MACHINE ===

    public void StartSaga()
    {
        if (Status != PixTransactionStatus.Approved)
            throw new InvalidOperationException("Apenas transações Approved podem iniciar a saga");
        Status = PixTransactionStatus.Pending;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkSourceDebited()
    {
        if (Status != PixTransactionStatus.Pending)
            throw new InvalidOperationException("Apenas transações Pending podem ser debitadas");
        SourceDebited = true;
        Status = PixTransactionStatus.SourceDebited;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        if (Status != PixTransactionStatus.SourceDebited)
            throw new InvalidOperationException("Source deve ser debitado antes de completar");
        DestinationCredited = true;
        Status = PixTransactionStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string reason)
    {
        FailureReason = reason;
        Status = PixTransactionStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Compensate(string reason)
    {
        FailureReason = reason;
        Status = PixTransactionStatus.Compensated;
        SourceDebited = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
