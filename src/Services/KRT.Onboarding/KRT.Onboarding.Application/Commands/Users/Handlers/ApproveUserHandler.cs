using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class ApproveUserHandler : IRequestHandler<ApproveUserCommand, ApproveUserResult>
{
    private readonly IAppUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly IKeycloakAdminService _keycloak;
    private readonly ILogger<ApproveUserHandler> _logger;

    public ApproveUserHandler(
        IAppUserRepository userRepo,
        IEmailService emailService,
        IKeycloakAdminService keycloak,
        ILogger<ApproveUserHandler> logger)
    {
        _userRepo = userRepo;
        _emailService = emailService;
        _keycloak = keycloak;
        _logger = logger;
    }

    public async Task<ApproveUserResult> Handle(ApproveUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user == null)
            return new ApproveUserResult(false, "Usuário não encontrado");

        user.Approve(request.AdminId.ToString());

        // Sync with Keycloak (best-effort)
        try
        {
            var keycloakResult = await _keycloak.CreateUserAsync(
                username: user.Document,
                email: user.Email,
                firstName: user.FullName,
                password: "", // user already has password hash, Keycloak won't be used for auth
                ct: cancellationToken);

            if (keycloakResult.Success && keycloakResult.UserId != null)
            {
                user.SetKeycloakUserId(keycloakResult.UserId);
                _logger.LogInformation("[Approve] Keycloak user created: {KeycloakId}", keycloakResult.UserId);
            }
            else
            {
                _logger.LogWarning("[Approve] Keycloak sync failed: {Error}. Continuing without Keycloak.", keycloakResult.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Approve] Keycloak sync failed. Continuing without Keycloak.");
        }

        await _userRepo.UpdateAsync(user);

        await _emailService.SendApprovalNotificationAsync(user.Email, user.FullName, user.Email, user.Document);

        _logger.LogInformation("[Approve] User approved: {UserId} by admin {AdminId}", user.Id, request.AdminId);

        return new ApproveUserResult(true, "Usuário aprovado com sucesso");
    }
}
