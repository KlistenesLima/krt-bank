namespace KRT.Onboarding.Application.Accounts.DTOs.Responses;

public record AccountResponse(
    Guid AccountId,
    string CustomerName,
    string CustomerDocument,
    string CustomerEmail,
    string Status,
    string Type,
    decimal Balance
);

public record BalanceResponse(
    Guid AccountId,
    decimal AvailableAmount,
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
