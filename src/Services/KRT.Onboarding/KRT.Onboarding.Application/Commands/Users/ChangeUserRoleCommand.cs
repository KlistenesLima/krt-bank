using KRT.Onboarding.Domain.Enums;
using MediatR;

namespace KRT.Onboarding.Application.Commands.Users;

public record ChangeUserRoleCommand(Guid UserId, UserRole NewRole, Guid AdminId) : IRequest<ChangeUserRoleResult>;
public record ChangeUserRoleResult(bool Success, string? Message);
