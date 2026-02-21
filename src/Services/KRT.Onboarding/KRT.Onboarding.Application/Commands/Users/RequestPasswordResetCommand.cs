using MediatR;

namespace KRT.Onboarding.Application.Commands.Users;

public record RequestPasswordResetCommand(string Email) : IRequest<RequestPasswordResetResult>;
public record RequestPasswordResetResult(bool Success, string? Message);
