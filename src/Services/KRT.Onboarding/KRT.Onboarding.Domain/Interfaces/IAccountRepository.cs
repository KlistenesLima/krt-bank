using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Entities;

namespace KRT.Onboarding.Domain.Interfaces;

public interface IAccountRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task AddAsync(Account account, CancellationToken cancellationToken);
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Account?> GetByCpfAsync(string cpf, CancellationToken cancellationToken);
    Task<Account?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task CreateVirtualCardForAccountAsync(Guid accountId, string holderName, CancellationToken cancellationToken);
    Task<List<Account>> GetAllAsync(CancellationToken cancellationToken);
}
