using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces; // Agora usa a Interface, não o Contexto direto
using MediatR;

namespace KRT.Onboarding.Application.Commands;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, CommandResult>
{
    private readonly IAccountRepository _repository;

    public CreateAccountCommandHandler(IAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommandResult> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = new Account(request.CustomerName, request.CustomerDocument, request.CustomerEmail);

        // O Handler não sabe o que é EF Core ou DbContext, ele só manda salvar
        await _repository.AddAsync(account, cancellationToken);

        return CommandResult.Success(account.Id);
    }
}
