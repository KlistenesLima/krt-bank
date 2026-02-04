using AutoMapper;
using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Application.Accounts.DTOs.Requests;
using KRT.Onboarding.Application.Accounts.DTOs.Responses;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;

namespace KRT.Onboarding.Application.Accounts.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public AccountService(IAccountRepository accountRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<Guid>> CreateAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var existing = await _accountRepository.GetByCpfAsync(request.CustomerDocument, cancellationToken);
        if (existing != null) return Result.Fail<Guid>("CPF já cadastrado.");

        var account = new Account(request.CustomerName, request.CustomerDocument, request.CustomerEmail);
        
        await _accountRepository.AddAsync(account, cancellationToken);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Ok(account.Id);
    }

    public async Task<Result<AccountResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(id, cancellationToken);
        
        if (account == null) return Result.Fail<AccountResponse>("Conta não encontrada.");
        return Result.Ok(_mapper.Map<AccountResponse>(account));
    }

    // STUBS
    public Task<Result<AccountResponse>> GetByAccountNumberAsync(string accountNumber, CancellationToken cancellationToken) => Task.FromResult(Result.Fail<AccountResponse>("Not implemented"));
    public Task<Result<BalanceResponse>> GetBalanceAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Result.Fail<BalanceResponse>("Not implemented"));
    public Task<Result<StatementResponse>> GetStatementAsync(Guid id, DateTime start, DateTime end, CancellationToken cancellationToken) => Task.FromResult(Result.Fail<StatementResponse>("Not implemented"));
    public Task<Result> ActivateAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Result.Ok());
    public Task<Result> BlockAsync(Guid id, string reason, CancellationToken cancellationToken) => Task.FromResult(Result.Ok());
    public Task<Result> UnblockAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(Result.Ok());
    public Task<Result> CloseAsync(Guid id, string reason, CancellationToken cancellationToken) => Task.FromResult(Result.Ok());
    public Task<Result> UpdateAsync(UpdateAccountRequest request, CancellationToken cancellationToken) => Task.FromResult(Result.Ok());
    public Task<Result<TransactionResponse>> DebitAsync(DebitAccountRequest request, CancellationToken cancellationToken) => Task.FromResult(Result.Fail<TransactionResponse>("Not implemented"));
    public Task<Result<TransactionResponse>> CreditAsync(CreditAccountRequest request, CancellationToken cancellationToken) => Task.FromResult(Result.Fail<TransactionResponse>("Not implemented"));
    public Task<Result<TransactionResponse>> TransferAsync(TransferRequest request, CancellationToken cancellationToken) => Task.FromResult(Result.Fail<TransactionResponse>("Not implemented"));
}
