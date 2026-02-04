using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Application.Accounts.DTOs.Requests;
using KRT.Onboarding.Application.Accounts.DTOs.Responses;

namespace KRT.Onboarding.Application.Accounts.Services;

public interface IAccountService
{
    // Queries
    Task<Result<AccountResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<AccountResponse>> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken);
    Task<Result<BalanceResponse>> GetBalanceAsync(Guid id, CancellationToken cancellationToken);
    Task<Result<StatementResponse>> GetStatementAsync(Guid id, DateTime start, DateTime end, CancellationToken cancellationToken);

    // Commands
    Task<Result<Guid>> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken);
    Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> BlockAsync(Guid id, string reason, CancellationToken cancellationToken);
    Task<Result> UnblockAsync(Guid id, CancellationToken cancellationToken);
    Task<Result> CloseAsync(Guid id, string reason, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(UpdateAccountRequest request, CancellationToken cancellationToken);
    
    // Transactional Commands
    Task<Result<TransactionResponse>> DebitAsync(DebitAccountRequest request, CancellationToken cancellationToken);
    Task<Result<TransactionResponse>> CreditAsync(CreditAccountRequest request, CancellationToken cancellationToken);
    Task<Result<TransactionResponse>> TransferAsync(TransferRequest request, CancellationToken cancellationToken);
}
