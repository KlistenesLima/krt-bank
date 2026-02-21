using MediatR;

namespace KRT.Onboarding.Application.Commands.Users;

public record ResetPasswordCommand(string Email, string Code, string NewPassword) : IRequest<ResetPasswordResult>;
public record ResetPasswordResult(bool Success, string? Message);
