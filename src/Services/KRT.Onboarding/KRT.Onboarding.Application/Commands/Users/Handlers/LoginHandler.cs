using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
{
    private readonly IAppUserRepository _userRepo;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(
        IAppUserRepository userRepo,
        IConfiguration configuration,
        ILogger<LoginHandler> logger)
    {
        _userRepo = userRepo;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var input = request.EmailOrDocument.Trim();

        // Determine if input is email or document
        var user = input.Contains('@')
            ? await _userRepo.GetByEmailAsync(input.ToLowerInvariant())
            : await _userRepo.GetByDocumentAsync(input.Replace(".", "").Replace("-", ""));

        if (user == null)
            return new LoginResult(false, null, "Email/CPF ou senha inválidos", null);

        // Check password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new LoginResult(false, null, "Email/CPF ou senha inválidos", null);

        // Check status
        if (user.Status == UserStatus.PendingEmailConfirmation)
            return new LoginResult(false, null, "Confirme seu email antes de fazer login", null);

        if (user.Status == UserStatus.PendingApproval)
            return new LoginResult(false, null, "Seu cadastro está aguardando aprovação do administrador", null);

        if (user.Status == UserStatus.Rejected)
            return new LoginResult(false, null, "Seu cadastro foi rejeitado", null);

        if (user.Status == UserStatus.Inactive)
            return new LoginResult(false, null, "Sua conta está desativada. Entre em contato com o administrador", null);

        if (user.Status != UserStatus.Active)
            return new LoginResult(false, null, "Conta não está ativa", null);

        // Generate JWT
        var token = GenerateJwtToken(user.Id, user.Email, user.FullName, user.Role, user.Document);

        _logger.LogInformation("[Login] User logged in: {UserId} | Role: {Role}", user.Id, user.Role);

        return new LoginResult(true, token, null, user.Role);
    }

    private string GenerateJwtToken(Guid userId, string email, string fullName, UserRole role, string document)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "KRT-Bank-Super-Secret-Key-2026-Minimum-32-Chars!";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "KRT.Onboarding";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "KRT.Bank";
        var jwtExpiryMinutes = int.TryParse(_configuration["Jwt:ExpiryMinutes"], out var exp) ? exp : 480;

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name, fullName),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim("document", document),
            new Claim("role", role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(jwtExpiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
