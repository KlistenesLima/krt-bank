using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;

namespace KRT.Onboarding.Application.Commands;

public class FinancialTransactionHandlers : 
    IRequestHandler<DebitAccountCommand, CommandResult>,
    IRequestHandler<CreditAccountCommand, CommandResult>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public FinancialTransactionHandlers(IAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandResult> Handle(DebitAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null) return CommandResult.Failure("Conta não encontrada");

        try 
        {
            account.Debit(request.Amount);
            await _unitOfWork.CommitAsync(cancellationToken);
            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message);
        }
    }

    public async Task<CommandResult> Handle(CreditAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(request.AccountId, cancellationToken);
        if (account == null) return CommandResult.Failure("Conta não encontrada");

        try
        {
            account.Credit(request.Amount);
            await _unitOfWork.CommitAsync(cancellationToken);
            return CommandResult.Success();
        }
        catch (Exception ex)
        {
            return CommandResult.Failure(ex.Message);
        }
    }
}
