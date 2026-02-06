using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Infra.Data.Repositories;

public class PixTransactionRepository : IPixTransactionRepository
{
    private readonly PaymentsDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public PixTransactionRepository(PaymentsDbContext context) => _context = context;

    public async Task AddAsync(PixTransaction transaction)
        => await _context.PixTransactions.AddAsync(transaction);

    public async Task<PixTransaction?> GetByIdAsync(Guid id)
        => await _context.PixTransactions.FindAsync(id);

    public async Task<PixTransaction?> GetByIdempotencyKeyAsync(Guid idempotencyKey)
        => await _context.PixTransactions.FirstOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey);

    public async Task<List<PixTransaction>> GetByAccountIdAsync(Guid accountId, int page = 1, int pageSize = 20)
        => await _context.PixTransactions
            .Where(t => t.SourceAccountId == accountId || t.DestinationAccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public void Update(PixTransaction transaction)
        => _context.PixTransactions.Update(transaction);
}
