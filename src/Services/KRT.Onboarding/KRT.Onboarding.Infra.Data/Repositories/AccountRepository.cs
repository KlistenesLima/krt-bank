using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Interfaces;
using KRT.Onboarding.Infra.Data.Context;

namespace KRT.Onboarding.Infra.Data.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationDbContext _context;

    // O ApplicationDbContext deve implementar IUnitOfWork para isso funcionar diretamente.
    // Caso contrário, injetamos IUnitOfWork separadamente.
    // Neste modelo pragmático, assumimos que o Contexto é a UoW.
    public IUnitOfWork UnitOfWork => _context; 

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken)
    {
        // Apenas adiciona à memória do EF Core.
        // O Commit acontece no CommandHandler via UnitOfWork.CommitAsync()
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByCpfAsync(string cpf, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            // CORREÇÃO: A propriedade na Entidade é Document, não Cpf
            .FirstOrDefaultAsync(a => a.Document == cpf, cancellationToken);
    }
}
