using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;
using KRT.BuildingBlocks.Domain;
using MediatR;

namespace KRT.Onboarding.Application.Commands;

public class PerformPixCommandHandler : IRequestHandler<PerformPixCommand, CommandResult>
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork; // Injeção da Interface Genérica

    public PerformPixCommandHandler(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandResult> Handle(PerformPixCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.AccountId, cancellationToken);
        
        if (account == null) 
            return CommandResult.Fail(new Dictionary<string, string[]> { { "Account", new[] { "Conta não encontrada" } } });

        if (account.Balance < request.Amount && account.Balance == 0)
        {
            account.Credit(1000); 
            await _transactionRepository.AddAsync(new Transaction(account.Id, 1000, "Depósito", "Bônus Inicial KRT"), cancellationToken);
        }

        if (account.Balance < request.Amount)
             return CommandResult.Fail(new Dictionary<string, string[]> { { "Balance", new[] { "Saldo insuficiente" } } });

        account.Debit(request.Amount);

        var tx = new Transaction(account.Id, request.Amount * -1, "Pix Enviado", $"Para: {request.PixKey}");
        await _transactionRepository.AddAsync(tx, cancellationToken);

        // Agora sim, persistência atômica via Interface
        await _unitOfWork.CommitAsync(cancellationToken);

        return CommandResult.Success(tx.Id);
    }
}
