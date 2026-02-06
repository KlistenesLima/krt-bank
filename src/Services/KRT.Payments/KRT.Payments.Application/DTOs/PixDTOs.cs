namespace KRT.Payments.Application.DTOs;

// === REQUEST ===
public record PixTransferRequest(
    Guid SourceAccountId,
    Guid DestinationAccountId,
    string PixKey,
    decimal Amount,
    string? Description,
    Guid IdempotencyKey
);

// === RESPONSE ===
public record PixTransferResponse(
    Guid TransactionId,
    string Status,
    decimal Amount,
    string Currency,
    DateTime CreatedAt,
    DateTime? CompletedAt
);

// === EXTRATO ===
public record TransactionHistoryResponse(
    Guid TransactionId,
    Guid SourceAccountId,
    Guid DestinationAccountId,
    decimal Amount,
    string Status,
    string? Description,
    DateTime CreatedAt
);

// === Client DTOs (para chamar o Onboarding) ===
public record AccountDebitRequest(decimal Amount, string Reason);
public record AccountCreditRequest(decimal Amount, string Reason);
public record AccountOperationResponse(bool Success, string? Error, decimal NewBalance);
