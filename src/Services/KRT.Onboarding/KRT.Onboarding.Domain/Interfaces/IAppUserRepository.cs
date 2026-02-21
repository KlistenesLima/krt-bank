using KRT.Onboarding.Domain.Entities;

namespace KRT.Onboarding.Domain.Interfaces;

public interface IAppUserRepository
{
    Task<AppUser?> GetByIdAsync(Guid id);
    Task<AppUser?> GetByEmailAsync(string email);
    Task<AppUser?> GetByDocumentAsync(string document);
    Task<List<AppUser>> GetAllAsync();
    Task<List<AppUser>> GetPendingApprovalAsync();
    Task AddAsync(AppUser user);
    Task UpdateAsync(AppUser user);
}
