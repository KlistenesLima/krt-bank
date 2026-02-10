using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;

namespace KRT.Onboarding.Domain.Interfaces;

public interface IPixKeyRepository
{
    Task<PixKey?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PixKey?> GetByKeyAsync(PixKeyType keyType, string keyValue, CancellationToken ct);
    Task<IEnumerable<PixKey>> GetByAccountIdAsync(Guid accountId, CancellationToken ct);
    Task<bool> ExistsAsync(PixKeyType keyType, string keyValue, CancellationToken ct);
    Task<int> CountByAccountIdAsync(Guid accountId, CancellationToken ct);
    Task AddAsync(PixKey pixKey, CancellationToken ct);
    void Update(PixKey pixKey);
    Task SaveChangesAsync(CancellationToken ct);
}
