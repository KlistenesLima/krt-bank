using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;
using KRT.Onboarding.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace KRT.Onboarding.Infra.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;
    public IUnitOfWork UnitOfWork => _context;

    public AccountRepository(ApplicationDbContext context) => _context = context;

    public async Task AddAsync(Account account, CancellationToken ct)
        => await _context.Accounts.AddAsync(account, ct);

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken ct)
        => await _context.Accounts.FindAsync(new object[] { id }, ct);

    public async Task<Account?> GetByCpfAsync(string cpf, CancellationToken ct)
        => await _context.Accounts.FirstOrDefaultAsync(a => a.Document == cpf, ct);

    public async Task<List<Account>> GetAllAsync(CancellationToken ct)
        => await _context.Accounts.OrderByDescending(a => a.CreatedAt).Take(100).ToListAsync(ct);
}
