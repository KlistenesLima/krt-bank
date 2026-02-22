namespace KRT.Payments.Application.Interfaces;

/// <summary>
/// Creates statement entries for completed PIX transactions.
/// </summary>
public interface IPixStatementService
{
    Task CreatePixStatementEntriesAsync(
        Guid sourceAccountId,
        Guid destinationAccountId,
        decimal amount,
        string pixKey,
        string? description,
        CancellationToken ct);
}
