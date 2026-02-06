namespace KRT.Payments.Application.Interfaces;

public record AccountOperationResult(bool Success, string? Error, decimal NewBalance);

public interface IOnboardingServiceClient
{
    Task<AccountOperationResult> DebitAccountAsync(Guid accountId, decimal amount, string reason, CancellationToken ct = default);
    Task<AccountOperationResult> CreditAccountAsync(Guid accountId, decimal amount, string reason, CancellationToken ct = default);
}
