using KRT.Onboarding.Application.Commands;
using KRT.Onboarding.Application.Commands.Users;
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
    /// Registro de novo usuário (2 etapas: email + aprovação admin).
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterUserCommand(request.FullName, request.Email, request.Document, request.Password);
        var result = await _mediator.Send(command);

        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Created("", new
        {
            success = true,
            userId = result.UserId,
            message = result.Message
        });
    }

    /// <summary>
    /// Confirmação de email com código de 6 dígitos.
    /// </summary>
    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequest request)
    {
        var command = new ConfirmEmailCommand(request.Email, request.Code);
        var result = await _mediator.Send(command);

        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Login via email/CPF + senha → JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var login = request.ResolvedLogin;
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { success = false, message = "Email/CPF e senha são obrigatórios" });

        var command = new LoginCommand(login, request.Password);
        var result = await _mediator.Send(command);

        if (!result.Success)
            return Unauthorized(new { success = false, message = result.Message });

        // Fetch account info for the frontend
        var cleanLogin = login.Contains('@')
            ? login.Trim().ToLowerInvariant()
            : login.Replace(".", "").Replace("-", "").Trim();
        var account = login.Contains('@')
            ? await _repository.GetByEmailAsync(cleanLogin, CancellationToken.None)
            : await _repository.GetByCpfAsync(cleanLogin, CancellationToken.None);

        return Ok(new
        {
            success = true,
            token = result.Token,
            accessToken = result.Token,
            role = result.Role?.ToString(),
            account = account != null ? new
            {
                id = account.Id,
                name = account.CustomerName,
                document = account.Document,
                email = account.Email,
                balance = account.Balance,
                status = account.Status.ToString(),
                role = account.Role ?? "User"
            } : null
        });
    }

    /// <summary>
    /// Solicitar recuperação de senha.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new RequestPasswordResetCommand(request.Email);
        var result = await _mediator.Send(command);

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Redefinir senha com código de recuperação.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request.Email, request.Code, request.NewPassword);
        var result = await _mediator.Send(command);

        if (!result.Success)
            return BadRequest(new { success = false, message = result.Message });

        return Ok(new { success = true, message = result.Message });
    }

    /// <summary>
    /// Login legado via Keycloak (CPF + senha) — mantido para compatibilidade.
    /// </summary>
    [HttpPost("login-keycloak")]
    public async Task<IActionResult> LoginKeycloak([FromBody] KeycloakLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Cpf) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { success = false, error = "CPF e senha são obrigatórios" });

        var cleanCpf = request.Cpf.Replace(".", "").Replace("-", "").Trim();

        var account = await _repository.GetByCpfAsync(cleanCpf, CancellationToken.None);
        if (account == null)
            return Unauthorized(new { success = false, error = "CPF ou senha inválidos" });

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

    /// <summary>
    /// Registro legado via Keycloak — mantido para compatibilidade.
    /// </summary>
    [HttpPost("register-keycloak")]
    public async Task<IActionResult> RegisterKeycloak([FromBody] CreateAccountCommand command)
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
}

// Request DTOs
public record RegisterRequest(string FullName, string Email, string Document, string Password);
public record ConfirmEmailRequest(string Email, string Code);
public class LoginRequest
{
    public string? EmailOrDocument { get; set; }
    public string? Identifier { get; set; }
    public string Password { get; set; } = "";
    public string ResolvedLogin => !string.IsNullOrWhiteSpace(EmailOrDocument) ? EmailOrDocument : Identifier ?? "";
}
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Email, string Code, string NewPassword);
public record KeycloakLoginRequest(string Cpf, string Password);
