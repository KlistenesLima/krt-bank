using KRT.Onboarding.Domain.Interfaces;
using MediatR;

namespace KRT.Onboarding.Application.Commands.Users.Handlers;

public class GetAllUsersHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
{
    private readonly IAppUserRepository _userRepo;

    public GetAllUsersHandler(IAppUserRepository userRepo) => _userRepo = userRepo;

    public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepo.GetAllAsync();
        return users.Select(u => new UserDto(
            u.Id, u.FullName, u.Email, u.Document,
            u.Role.ToString(), u.Status.ToString(), u.CreatedAt, u.ApprovedAt)).ToList();
    }
}

public class GetPendingUsersHandler : IRequestHandler<GetPendingUsersQuery, List<UserDto>>
{
    private readonly IAppUserRepository _userRepo;

    public GetPendingUsersHandler(IAppUserRepository userRepo) => _userRepo = userRepo;

    public async Task<List<UserDto>> Handle(GetPendingUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepo.GetPendingApprovalAsync();
        return users.Select(u => new UserDto(
            u.Id, u.FullName, u.Email, u.Document,
            u.Role.ToString(), u.Status.ToString(), u.CreatedAt, u.ApprovedAt)).ToList();
    }
}

public class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    private readonly IAppUserRepository _userRepo;

    public GetUserByIdHandler(IAppUserRepository userRepo) => _userRepo = userRepo;

    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.Id);
        if (user == null) return null;
        return new UserDto(
            user.Id, user.FullName, user.Email, user.Document,
            user.Role.ToString(), user.Status.ToString(), user.CreatedAt, user.ApprovedAt);
    }
}
