using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;

namespace KRT.Payments.Domain.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    IUnitOfWork UnitOfWork { get; } // Adicionado contrato
    Task AddAsync(Payment payment);
    Task<Payment?> GetByIdAsync(Guid id);
}
