using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using KRT.Onboarding.Domain.Interfaces;
using KRT.Onboarding.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace KRT.Onboarding.Infra.Data.Repositories;

public class PixKeyRepository : IPixKeyRepository
{
    private readonly ApplicationDbContext _context;

    public PixKeyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PixKey?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.PixKeys
            .Include(pk => pk.Account)
            .FirstOrDefaultAsync(pk => pk.Id == id && pk.IsActive, ct);
    }

    public async Task<PixKey?> GetByKeyAsync(PixKeyType keyType, string keyValue, CancellationToken ct)
    {
        var normalized = NormalizeForSearch(keyType, keyValue);
        return await _context.PixKeys
            .Include(pk => pk.Account)
            .FirstOrDefaultAsync(pk =>
                pk.KeyType == keyType &&
                pk.KeyValue == normalized &&
                pk.IsActive, ct);
    }

    public async Task<IEnumerable<PixKey>> GetByAccountIdAsync(Guid accountId, CancellationToken ct)
    {
        return await _context.PixKeys
            .Where(pk => pk.AccountId == accountId && pk.IsActive)
            .OrderBy(pk => pk.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsAsync(PixKeyType keyType, string keyValue, CancellationToken ct)
    {
        var normalized = NormalizeForSearch(keyType, keyValue);
        return await _context.PixKeys
            .AnyAsync(pk =>
                pk.KeyType == keyType &&
                pk.KeyValue == normalized &&
                pk.IsActive, ct);
    }

    public async Task<int> CountByAccountIdAsync(Guid accountId, CancellationToken ct)
    {
        return await _context.PixKeys
            .CountAsync(pk => pk.AccountId == accountId && pk.IsActive, ct);
    }

    public async Task AddAsync(PixKey pixKey, CancellationToken ct)
    {
        await _context.PixKeys.AddAsync(pixKey, ct);
    }

    public void Update(PixKey pixKey)
    {
        _context.PixKeys.Update(pixKey);
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _context.SaveChangesAsync(ct);
    }

    private static string NormalizeForSearch(PixKeyType keyType, string keyValue)
    {
        return keyType switch
        {
            PixKeyType.Cpf => new string(keyValue.Where(char.IsDigit).ToArray()),
            PixKeyType.Email => keyValue.Trim().ToLowerInvariant(),
            PixKeyType.Phone => keyValue.StartsWith("+55")
                ? keyValue.Trim()
                : "+55" + new string(keyValue.Where(char.IsDigit).ToArray()),
            PixKeyType.Random => keyValue.Trim(),
            _ => keyValue.Trim()
        };
    }
}
