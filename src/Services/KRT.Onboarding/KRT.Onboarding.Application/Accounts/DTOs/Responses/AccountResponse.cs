namespace KRT.Onboarding.Application.Accounts.DTOs.Responses;

public record AccountResponse(
    Guid AccountId,
    string Number,
    string BranchCode,
    string CustomerId,
    string CustomerName,
    string CustomerDocument,
    string CustomerEmail,
    string Status,
    string Type,
    string Currency,
    decimal Balance,
    decimal AvailableBalance
);

public record BalanceResponse(
    Guid AccountId,
    decimal AvailableAmount,
    decimal BlockedAmount,
    string Currency,
    DateTime UpdatedAt
);

public record TransactionResponse(
    Guid TransactionId,
    string Type,
    decimal Amount,
    string Currency,
    string Status,
    DateTime CreatedAt,
    string Description
);

public record StatementResponse(
    Guid AccountId,
    DateTime StartDate,
    DateTime EndDate,
    List<TransactionResponse> Transactions
);
