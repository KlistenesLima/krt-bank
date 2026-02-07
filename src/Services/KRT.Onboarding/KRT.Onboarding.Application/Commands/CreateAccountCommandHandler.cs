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

        // 1. Cria usuário no Keycloak (username = CPF sem pontos)
        var keycloakResult = await _keycloak.CreateUserAsync(
            username: cleanCpf,
            email: email,
            firstName: request.CustomerName,
            password: request.Password,
            ct: cancellationToken);

        if (!keycloakResult.Success)
        {
            _logger.LogWarning("Keycloak user creation failed: {Error}", keycloakResult.Error);
            return CommandResult.Failure(keycloakResult.Error ?? "Falha ao criar usuário de autenticação");
        }

        // 2. Cria conta bancária
        var account = new Account(
            request.CustomerName,
            cleanCpf,
            email,
            request.CustomerPhone.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", ""),
            AccountType.Checking);

        await _repository.AddAsync(account, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Conta criada: {AccountId} | CPF: {Cpf} | Keycloak: {KeycloakId}",
            account.Id, cleanCpf, keycloakResult.UserId);

        return CommandResult.Success(account.Id);
    }
}
