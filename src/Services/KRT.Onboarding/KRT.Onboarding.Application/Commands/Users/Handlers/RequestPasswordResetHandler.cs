using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetCommand, RequestPasswordResetResult>
{
    private readonly IAppUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<RequestPasswordResetHandler> _logger;

    public RequestPasswordResetHandler(
        IAppUserRepository userRepo,
        IEmailService emailService,
        ILogger<RequestPasswordResetHandler> logger)
    {
        _userRepo = userRepo;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<RequestPasswordResetResult> Handle(RequestPasswordResetCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user == null)
            return new RequestPasswordResetResult(true, "Se o email estiver cadastrado, você receberá um código de recuperação.");

        user.SetPasswordResetCode();
        await _userRepo.UpdateAsync(user);

        await _emailService.SendPasswordResetAsync(user.Email, user.FullName, user.PasswordResetCode!);

        _logger.LogInformation("[PasswordReset] Reset code sent to: {Email}", user.Email);

        return new RequestPasswordResetResult(true, "Se o email estiver cadastrado, você receberá um código de recuperação.");
    }
}
