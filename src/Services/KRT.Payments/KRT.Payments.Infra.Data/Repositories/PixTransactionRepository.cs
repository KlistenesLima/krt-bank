using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Enums;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Infra.Data.Repositories;

public class PixTransactionRepository : IPixTransactionRepository
{
    private readonly PaymentsDbContext _context;

    public PixTransactionRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public IUnitOfWork UnitOfWork => _context;

    public async Task AddAsync(PixTransaction transaction, CancellationToken ct = default)
    {
        await _context.PixTransactions.AddAsync(transaction, ct);
    }

    public async Task<PixTransaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PixTransactions.FindAsync(new object[] { id }, ct);
    }

    public async Task<PixTransaction?> GetByIdempotencyKeyAsync(Guid idempotencyKey, CancellationToken ct = default)
    {
        return await _context.PixTransactions
            .FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey, ct);
    }

    public async Task<List<PixTransaction>> GetByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20)
    {
        return await _context.PixTransactions
            .Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<PixTransaction>> GetByStatusAsync(
        PixTransactionStatus status, int limit = 10, CancellationToken ct = default)
    {
        return await _context.PixTransactions
            .Where(t => t.Status == status)
            .OrderBy(t => t.CreatedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    public void Update(PixTransaction transaction)
    {
        _context.PixTransactions.Update(transaction);
    }
}
