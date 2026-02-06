using KRT.BuildingBlocks.Domain; // Para IUnitOfWork
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums; // Para AccountType
using KRT.Onboarding.Domain.Interfaces;
using MediatR;

namespace KRT.Onboarding.Application.Commands;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, CommandResult>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork; // INJEÇÃO NECESSÁRIA

    public CreateAccountCommandHandler(IAccountRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CommandResult> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // 1. Instanciação corrigida com AccountType.Checking
        var account = new Account(request.CustomerName, request.CustomerDocument, request.CustomerEmail, AccountType.Checking);

        // 2. Adiciona ao Repositório (Memória)
        await _repository.AddAsync(account, cancellationToken);

        // 3. CRÍTICO: Commit no Banco de Dados
        await _unitOfWork.CommitAsync(cancellationToken);

        return CommandResult.Success(account.Id);
    }
}
