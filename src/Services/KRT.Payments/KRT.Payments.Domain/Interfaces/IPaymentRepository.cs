using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Models;

namespace KRT.Payments.Domain.Interfaces;

public interface IPaymentRepository
{
    IUnitOfWork UnitOfWork { get; }
    Task<AccountView?> GetAccountViewAsync(Guid accountId);
    Task AddTransactionAsync(PixTransaction transaction);
}
