using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;
using KRT.Onboarding.Infra.Data.Context;

namespace KRT.Onboarding.Infra.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;
    
    // IUnitOfWork da interface IRepository
    public IUnitOfWork UnitOfWork => _context; 

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Account account)
    {
        await _context.Accounts.AddAsync(account);
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Account?> GetByCpfAsync(string cpf)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Cpf == cpf);
    }

    public void Dispose() => _context.Dispose();
}
