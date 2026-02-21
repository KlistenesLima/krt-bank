using AutoMapper;
using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Application.Accounts.DTOs.Requests;
using KRT.Onboarding.Application.Accounts.DTOs.Responses;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
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

    public async Task<Result<Guid>> CreateAsync(CreateAccountRequest request, CancellationToken ct)
    {
        var existing = await _accountRepository.GetByCpfAsync(request.CustomerDocument, ct);
        if (existing != null) return Result.Fail<Guid>("CPF já cadastrado.");

        var account = new Account(request.CustomerName, request.CustomerDocument, request.CustomerEmail, "", AccountType.Checking);
        await _accountRepository.AddAsync(account, ct);
        await _unitOfWork.CommitAsync(ct);

        return Result.Ok(account.Id);
    }

    public async Task<Result<AccountResponse>> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account == null) return Result.Fail<AccountResponse>("Conta não encontrada.");
        return Result.Ok(_mapper.Map<AccountResponse>(account));
    }

    public async Task<Result<BalanceResponse>> GetBalanceAsync(Guid id, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account == null) return Result.Fail<BalanceResponse>("Conta não encontrada.");
        return Result.Ok(new BalanceResponse(account.Id, account.Balance, "BRL", DateTime.UtcNow));
    }

    public async Task<Result> ActivateAsync(Guid id, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account == null) return Result.Fail("Conta não encontrada.");
        account.Activate();
        await _unitOfWork.CommitAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> BlockAsync(Guid id, string reason, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account == null) return Result.Fail("Conta não encontrada.");
        account.Block(reason);
        await _unitOfWork.CommitAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> UnblockAsync(Guid id, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account == null) return Result.Fail("Conta não encontrada.");
        account.Activate();
        await _unitOfWork.CommitAsync(ct);
        return Result.Ok();
    }

    public async Task<Result> CloseAsync(Guid id, string reason, CancellationToken ct)
    {
        var account = await _accountRepository.GetByIdAsync(id, ct);
        if (account == null) return Result.Fail("Conta não encontrada.");
        account.Close(reason);
        await _unitOfWork.CommitAsync(ct);
        return Result.Ok();
    }

    // Stubs
    public Task<Result<AccountResponse>> GetByAccountNumberAsync(string accountNumber, CancellationToken ct)
        => Task.FromResult(Result.Fail<AccountResponse>("Not implemented"));

    public Task<Result<StatementResponse>> GetStatementAsync(Guid id, DateTime start, DateTime end, CancellationToken ct)
        => Task.FromResult(Result.Fail<StatementResponse>("Not implemented"));

    public Task<Result> UpdateAsync(UpdateAccountRequest request, CancellationToken ct)
        => Task.FromResult(Result.Ok());

    public Task<Result<TransactionResponse>> DebitAsync(DebitAccountRequest request, CancellationToken ct)
        => Task.FromResult(Result.Fail<TransactionResponse>("Use endpoint /accounts/{id}/debit"));

    public Task<Result<TransactionResponse>> CreditAsync(CreditAccountRequest request, CancellationToken ct)
        => Task.FromResult(Result.Fail<TransactionResponse>("Use endpoint /accounts/{id}/credit"));

    public Task<Result<TransactionResponse>> TransferAsync(TransferRequest request, CancellationToken ct)
        => Task.FromResult(Result.Fail<TransactionResponse>("Use Pix service"));
}

