using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Entities;

namespace KRT.Onboarding.Domain.Interfaces;

public interface IAccountRepository : IRepository<Account>
{
    Task<Account?> GetByCpfAsync(string cpf);
    Task<Account?> GetByIdAsync(Guid id);
    Task AddAsync(Account account);
}
