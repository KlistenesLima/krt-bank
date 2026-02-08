using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Application.Commands;

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, CommandResult>
{
    private readonly IAccountRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IKeycloakAdminService _keycloak;
    private readonly ILogger<CreateAccountCommandHandler> _logger;

    public CreateAccountCommandHandler(
        IAccountRepository repository,
        IUnitOfWork unitOfWork,
        IKeycloakAdminService keycloak,
        ILogger<CreateAccountCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _keycloak = keycloak;
        _logger = logger;
    }

    public async Task<CommandResult> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        // Limpa CPF
        var cleanCpf = request.CustomerDocument.Replace(".", "").Replace("-", "").Trim();
        var email = request.CustomerEmail.Trim().ToLower();

        // 1. Cria usuario no Keycloak (username = CPF sem pontos)
        var keycloakResult = await _keycloak.CreateUserAsync(
            username: cleanCpf,
            email: email,
            firstName: request.CustomerName,
            password: request.Password,
            ct: cancellationToken);

        if (!keycloakResult.Success)
        {
            _logger.LogWarning("Keycloak user creation failed: {Error}", keycloakResult.Error);
            return CommandResult.Failure(keycloakResult.Error ?? "Falha ao criar usuario de autenticacao");
        }

        // 2. Cria conta bancaria
        var account = new Account(
            request.CustomerName,
            cleanCpf,
            email,
            request.CustomerPhone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", ""),
            AccountType.Checking);

        // 3. BONUS DEMO: Credita R$ 1.000,00 para a conta recem-criada
        account.Credit(1000.00m);

        await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Conta criada com bonus: {AccountId} | CPF: {Cpf} | Saldo: R$ {Balance} | Keycloak: {KeycloakId}",
            account.Id, cleanCpf, account.Balance, keycloakResult.UserId);

        return CommandResult.Success(account.Id);
    }
}