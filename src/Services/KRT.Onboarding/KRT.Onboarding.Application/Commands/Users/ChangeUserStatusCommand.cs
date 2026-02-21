using MediatR;

namespace KRT.Onboarding.Application.Commands.Users;

public record ChangeUserStatusCommand(Guid UserId, bool Activate, Guid AdminId) : IRequest<ChangeUserStatusResult>;
public record ChangeUserStatusResult(bool Success, string? Message);
