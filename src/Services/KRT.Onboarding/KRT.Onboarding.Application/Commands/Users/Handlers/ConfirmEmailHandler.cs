using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class ConfirmEmailHandler : IRequestHandler<ConfirmEmailCommand, ConfirmEmailResult>
{
    private readonly IAppUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<ConfirmEmailHandler> _logger;

    public ConfirmEmailHandler(
        IAppUserRepository userRepo,
        IEmailService emailService,
        ILogger<ConfirmEmailHandler> logger)
    {
        _userRepo = userRepo;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ConfirmEmailResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user == null)
            return new ConfirmEmailResult(false, "Usuário não encontrado");

        if (user.EmailConfirmationCode != request.Code)
            return new ConfirmEmailResult(false, "Código de confirmação inválido");

        if (user.EmailConfirmationExpiry.HasValue && user.EmailConfirmationExpiry < DateTime.UtcNow)
            return new ConfirmEmailResult(false, "Código de confirmação expirado. Solicite um novo.");

        user.ConfirmEmail();
        await _userRepo.UpdateAsync(user);

        await _emailService.SendRegistrationPendingAsync(user.Email, user.FullName);

        _logger.LogInformation("[ConfirmEmail] Email confirmed for user: {UserId}", user.Id);

        return new ConfirmEmailResult(true, "Email confirmado! Aguarde aprovação do administrador.");
    }
}
