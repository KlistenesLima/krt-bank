using KRT.Onboarding.Domain.Enums;
using MediatR;

namespace KRT.Onboarding.Application.Commands.Users;

public record GetAllUsersQuery() : IRequest<List<UserDto>>;
public record GetPendingUsersQuery() : IRequest<List<UserDto>>;
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto?>;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    string Document,
    UserRole Role,
    UserStatus Status,
    DateTime CreatedAt,
    DateTime? ApprovedAt);
