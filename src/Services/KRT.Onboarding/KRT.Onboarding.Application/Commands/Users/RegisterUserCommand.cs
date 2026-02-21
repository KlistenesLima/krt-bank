using MediatR;

namespace KRT.Onboarding.Application.Commands.Users;

public record RegisterUserCommand(string FullName, string Email, string Document, string Password) : IRequest<RegisterUserResult>;
public record RegisterUserResult(bool Success, string? Message, Guid? UserId);
