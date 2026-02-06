using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Application.Accounts.DTOs.Requests;
using KRT.Onboarding.Application.Accounts.DTOs.Responses;

namespace KRT.Onboarding.Application.Accounts.Services;

public interface IAccountService
{
    // Queries
    Task<Result<AccountResponse>> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Result<AccountResponse>> GetByAccountNumberAsync(string accountNumber, CancellationToken ct);
    Task<Result<BalanceResponse>> GetBalanceAsync(Guid id, CancellationToken ct);
    Task<Result<StatementResponse>> GetStatementAsync(Guid id, DateTime start, DateTime end, CancellationToken ct);

    // Commands
    Task<Result<Guid>> CreateAsync(CreateAccountRequest request, CancellationToken ct);
    Task<Result> ActivateAsync(Guid id, CancellationToken ct);
    Task<Result> BlockAsync(Guid id, string reason, CancellationToken ct);
    Task<Result> UnblockAsync(Guid id, CancellationToken ct);
    Task<Result> CloseAsync(Guid id, string reason, CancellationToken ct);
    Task<Result> UpdateAsync(UpdateAccountRequest request, CancellationToken ct);

    // Transactional
    Task<Result<TransactionResponse>> DebitAsync(DebitAccountRequest request, CancellationToken ct);
    Task<Result<TransactionResponse>> CreditAsync(CreditAccountRequest request, CancellationToken ct);
    Task<Result<TransactionResponse>> TransferAsync(TransferRequest request, CancellationToken ct);
}
