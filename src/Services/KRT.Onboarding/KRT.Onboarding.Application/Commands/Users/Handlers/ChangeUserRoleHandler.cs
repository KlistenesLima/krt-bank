using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class ChangeUserRoleHandler : IRequestHandler<ChangeUserRoleCommand, ChangeUserRoleResult>
{
    private readonly IAppUserRepository _userRepo;
    private readonly ILogger<ChangeUserRoleHandler> _logger;

    public ChangeUserRoleHandler(IAppUserRepository userRepo, ILogger<ChangeUserRoleHandler> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<ChangeUserRoleResult> Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user == null)
            return new ChangeUserRoleResult(false, "Usuário não encontrado");

        user.ChangeRole(request.NewRole);
        await _userRepo.UpdateAsync(user);

        _logger.LogInformation("[Role] User role changed to {Role}: {UserId} by admin {AdminId}",
            request.NewRole, user.Id, request.AdminId);

        return new ChangeUserRoleResult(true, $"Role alterado para {request.NewRole}");
    }
}
