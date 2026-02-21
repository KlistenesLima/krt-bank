using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class ChangeUserStatusHandler : IRequestHandler<ChangeUserStatusCommand, ChangeUserStatusResult>
{
    private readonly IAppUserRepository _userRepo;
    private readonly ILogger<ChangeUserStatusHandler> _logger;

    public ChangeUserStatusHandler(IAppUserRepository userRepo, ILogger<ChangeUserStatusHandler> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<ChangeUserStatusResult> Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user == null)
            return new ChangeUserStatusResult(false, "Usuário não encontrado");

        if (request.Activate)
            user.Activate();
        else
            user.Deactivate();

        await _userRepo.UpdateAsync(user);

        var action = request.Activate ? "ativado" : "desativado";
        _logger.LogInformation("[Status] User {Action}: {UserId} by admin {AdminId}", action, user.Id, request.AdminId);

        return new ChangeUserStatusResult(true, $"Usuário {action} com sucesso");
    }
}
