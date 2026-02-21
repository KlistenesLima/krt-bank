using KRT.Onboarding.Application.Interfaces;
using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class RejectUserHandler : IRequestHandler<RejectUserCommand, RejectUserResult>
{
    private readonly IAppUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly ILogger<RejectUserHandler> _logger;

    public RejectUserHandler(
        IAppUserRepository userRepo,
        IEmailService emailService,
        ILogger<RejectUserHandler> logger)
    {
        _userRepo = userRepo;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<RejectUserResult> Handle(RejectUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user == null)
            return new RejectUserResult(false, "Usuário não encontrado");

        user.Reject(request.AdminId.ToString());
        await _userRepo.UpdateAsync(user);

        await _emailService.SendRejectionNotificationAsync(user.Email, user.FullName);

        _logger.LogInformation("[Reject] User rejected: {UserId} by admin {AdminId}", user.Id, request.AdminId);

        return new RejectUserResult(true, "Usuário rejeitado");
    }
}
