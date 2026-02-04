using KRT.Onboarding.Domain.Entities;

namespace KRT.Onboarding.Domain.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken);
}
