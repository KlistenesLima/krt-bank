using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Interfaces;
using KRT.Onboarding.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace KRT.Onboarding.Infra.Data.Repositories;

public class AppUserRepository : IAppUserRepository
{
    private readonly ApplicationDbContext _context;

    public AppUserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AppUser?> GetByIdAsync(Guid id)
    {
        return await _context.AppUsers.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<AppUser?> GetByEmailAsync(string email)
    {
        return await _context.AppUsers.FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
    }

    public async Task<AppUser?> GetByDocumentAsync(string document)
    {
        var cleanDoc = document.Replace(".", "").Replace("-", "");
        return await _context.AppUsers.FirstOrDefaultAsync(u => u.Document == cleanDoc);
    }

    public async Task<List<AppUser>> GetAllAsync()
    {
        return await _context.AppUsers.OrderByDescending(u => u.CreatedAt).ToListAsync();
    }

    public async Task<List<AppUser>> GetPendingApprovalAsync()
    {
        return await _context.AppUsers
            .Where(u => u.Status == UserStatus.PendingApproval)
            .OrderBy(u => u.CreatedAt)
            .ToListAsync();
    }

    public async Task AddAsync(AppUser user)
    {
        await _context.AppUsers.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(AppUser user)
    {
        _context.AppUsers.Update(user);
        await _context.SaveChangesAsync();
    }
}
