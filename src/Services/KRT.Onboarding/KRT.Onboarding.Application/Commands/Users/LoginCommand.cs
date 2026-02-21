using KRT.Onboarding.Domain.Enums;
using MediatR;

namespace KRT.Onboarding.Application.Commands.Users;

public record LoginCommand(string EmailOrDocument, string Password) : IRequest<LoginResult>;
public record LoginResult(bool Success, string? Token, string? Message, UserRole? Role);
