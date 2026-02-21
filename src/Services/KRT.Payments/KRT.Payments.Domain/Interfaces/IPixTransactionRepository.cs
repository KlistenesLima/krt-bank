using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Enums;

namespace KRT.Payments.Domain.Interfaces;

public interface IPixTransactionRepository : IRepository<PixTransaction>
{
    IUnitOfWork UnitOfWork { get; }
    Task AddAsync(PixTransaction transaction, CancellationToken ct = default);
    Task<PixTransaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PixTransaction?> GetByIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken ct = default);
    Task<List<PixTransaction>> GetByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20);
    Task<List<PixTransaction>> GetByStatusAsync(PixTransactionStatus status, int limit = 10, CancellationToken ct = default);
    void Update(PixTransaction transaction);
}
