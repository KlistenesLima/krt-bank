using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;
using KRT.Onboarding.Infra.Data.Context;

namespace KRT.Onboarding.Infra.Data.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction)
    {
        await _context.Transactions.AddAsync(transaction);
    }
}
