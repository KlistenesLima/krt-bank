namespace KRT.Onboarding.Application.Accounts.DTOs.Requests;

public record CreateAccountRequest(
    string CustomerName, 
    string CustomerDocument, // Antes era Cpf
    string CustomerEmail,    // Antes era Email
    string BranchCode        // Adicionado
);

public record UpdateAccountRequest(Guid AccountId, string Email);

public record TransferRequest(
    Guid SourceAccountId,      // Antes era FromId
    Guid DestinationAccountId, // Antes era ToId
    decimal Amount,
    string Currency,           // Adicionado
    Guid IdempotencyKey        // Adicionado
);

public record DebitAccountRequest(Guid AccountId, decimal Amount);
public record CreditAccountRequest(Guid AccountId, decimal Amount);
public record BlockAccountRequest(Guid AccountId, string Reason);
