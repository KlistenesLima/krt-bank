using MediatR;

namespace KRT.Onboarding.Application.Commands.Users;

public record ApproveUserCommand(Guid UserId, Guid AdminId) : IRequest<ApproveUserResult>;
public record ApproveUserResult(bool Success, string? Message);
