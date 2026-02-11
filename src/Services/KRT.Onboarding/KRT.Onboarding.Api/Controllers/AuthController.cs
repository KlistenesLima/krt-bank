using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace KRT.Onboarding.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IKeycloakAdminService _keycloak;
    private readonly IAccountRepository _repository;

    public AuthController(IMediator mediator, IKeycloakAdminService keycloak, IAccountRepository repository)
    {
        _mediator = mediator;
        _keycloak = keycloak;
        _repository = repository;
    }

    /// <summary>
    /// Registro unificado: cria usuário no Keycloak + conta bancária.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] CreateAccountCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.IsValid)
            return BadRequest(new { success = false, errors = result.Errors });

        return Created("", new
        {
            success = true,
            accountId = result.Id,
            message = "Conta criada com sucesso! Faça login com seu CPF e senha."
        });
    }

    /// <summary>
    /// Login via Keycloak: CPF + senha → JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Cpf) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { success = false, error = "CPF e senha são obrigatórios" });

        var cleanCpf = request.Cpf.Replace(".", "").Replace("-", "").Trim();

        // Busca conta pelo CPF
        var account = await _repository.GetByCpfAsync(cleanCpf, CancellationToken.None);
        if (account == null)
            return Unauthorized(new { success = false, error = "CPF ou senha inválidos" });

        // Autentica no Keycloak (username = CPF)
        var tokenResult = await _keycloak.LoginAsync(cleanCpf, request.Password);

        if (!tokenResult.Success)
            return Unauthorized(new { success = false, error = tokenResult.Error ?? "CPF ou senha inválidos" });

        return Ok(new
        {
            success = true,
            accessToken = tokenResult.AccessToken,
            refreshToken = tokenResult.RefreshToken,
            expiresIn = tokenResult.ExpiresIn,
            account = new
            {
                id = account.Id,
                name = account.CustomerName,
                document = account.Document,
                email = account.Email,
                balance = account.Balance,
                status = account.Status.ToString(),
                role = account.Role ?? "User"
            }
        });
    }
}

public record LoginRequest(string Cpf, string Password);

