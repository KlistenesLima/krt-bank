using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;

namespace KRT.Payments.Domain.Interfaces;

public interface IPixTransactionRepository : IRepository<PixTransaction>
{
    IUnitOfWork UnitOfWork { get; }
    Task AddAsync(PixTransaction transaction);
    Task<PixTransaction?> GetByIdAsync(Guid id);
    Task<PixTransaction?> GetByIdempotencyKeyAsync(Guid idempotencyKey);
    Task<List<PixTransaction>> GetByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20);
    void Update(PixTransaction transaction);
}
