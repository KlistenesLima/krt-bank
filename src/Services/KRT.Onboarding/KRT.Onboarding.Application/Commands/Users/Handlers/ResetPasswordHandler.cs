using KRT.Onboarding.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly IAppUserRepository _userRepo;
    private readonly ILogger<ResetPasswordHandler> _logger;

    public ResetPasswordHandler(IAppUserRepository userRepo, ILogger<ResetPasswordHandler> logger)
    {
        _userRepo = userRepo;
        _logger = logger;
    }

    public async Task<ResetPasswordResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByEmailAsync(request.Email.Trim().ToLowerInvariant());
        if (user == null)
            return new ResetPasswordResult(false, "Usuário não encontrado");

        if (user.PasswordResetCode != request.Code)
            return new ResetPasswordResult(false, "Código de recuperação inválido");

        if (user.PasswordResetExpiry.HasValue && user.PasswordResetExpiry < DateTime.UtcNow)
            return new ResetPasswordResult(false, "Código de recuperação expirado. Solicite um novo.");

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.ResetPassword(newPasswordHash);
        await _userRepo.UpdateAsync(user);

        _logger.LogInformation("[PasswordReset] Password reset for user: {UserId}", user.Id);

        return new ResetPasswordResult(true, "Senha redefinida com sucesso");
    }
}
