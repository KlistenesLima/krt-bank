using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Infra.Data.Context;

namespace KRT.Payments.Infra.Data.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentsDbContext _context;

    public IUnitOfWork UnitOfWork => _context;

    public PaymentRepository(PaymentsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment)
    {
        await _context.Payments.AddAsync(payment);
    }

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _context.Payments.FindAsync(id);
    }
}
