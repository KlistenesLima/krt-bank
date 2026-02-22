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

    public AccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken)
    {
        await _context.Accounts.AddAsync(account, cancellationToken);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByCpfAsync(string cpf, CancellationToken cancellationToken)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Document == cpf, cancellationToken);
    }

    public async Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.Email == email, cancellationToken);
    }

    public async Task<List<Account>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _context.Accounts.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task CreateVirtualCardForAccountAsync(Guid accountId, string holderName, CancellationToken cancellationToken)
    {
        var rng = new Random();
        var cardNumber = "4532" + rng.Next(100000000, 999999999).ToString() + rng.Next(100, 999).ToString();
        var last4 = cardNumber[^4..];
        var cvv = rng.Next(100, 999).ToString();
        var expDate = DateTime.UtcNow.AddYears(5);

        await _context.Database.ExecuteSqlRawAsync(@"
            INSERT INTO ""VirtualCards"" (""Id"", ""AccountId"", ""CardNumber"", ""CardholderName"",
                ""ExpirationMonth"", ""ExpirationYear"", ""Cvv"", ""Last4Digits"", ""Brand"", ""Status"",
                ""SpendingLimit"", ""SpentThisMonth"", ""IsContactless"", ""IsOnlinePurchase"", ""IsInternational"",
                ""CvvExpiresAt"", ""CreatedAt"", ""UpdatedAt"")
            VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, 0, 0,
                15000.00, 0.00, true, true, false,
                NOW() + INTERVAL '24 hours', NOW(), NOW())
            ON CONFLICT DO NOTHING",
            Guid.NewGuid(), accountId, cardNumber, holderName.ToUpper(),
            expDate.ToString("MM"), expDate.ToString("yyyy"), cvv, last4);
    }
}
