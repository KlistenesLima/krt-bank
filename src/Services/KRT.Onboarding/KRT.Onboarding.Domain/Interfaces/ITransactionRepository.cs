using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Entities;

namespace KRT.Onboarding.Domain.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    Task AddAsync(Transaction transaction);
}
