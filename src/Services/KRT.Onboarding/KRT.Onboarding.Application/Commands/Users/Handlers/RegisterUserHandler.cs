using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class RegisterUserHandler : IRequestHandler<RegisterUserCommand, RegisterUserResult>
{
    private readonly IAppUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterUserHandler> _logger;

    public RegisterUserHandler(
        IAppUserRepository userRepo,
        IEmailService emailService,
        ILogger<RegisterUserHandler> logger)
    {
        _userRepo = userRepo;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var cleanDoc = request.Document.Replace(".", "").Replace("-", "").Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        var existingByEmail = await _userRepo.GetByEmailAsync(email);
        if (existingByEmail != null)
            return new RegisterUserResult(false, "Email já cadastrado", null);

        var existingByDoc = await _userRepo.GetByDocumentAsync(cleanDoc);
        if (existingByDoc != null)
            return new RegisterUserResult(false, "CPF já cadastrado", null);

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = AppUser.Create(request.FullName, email, cleanDoc, passwordHash);

        await _userRepo.AddAsync(user);

        await _emailService.SendEmailConfirmationAsync(user.Email, user.FullName, user.EmailConfirmationCode!);

        _logger.LogInformation("[Register] User created: {UserId} | Email: {Email}", user.Id, user.Email);

        return new RegisterUserResult(true, "Cadastro realizado! Verifique seu email para confirmar.", user.Id);
    }
}
