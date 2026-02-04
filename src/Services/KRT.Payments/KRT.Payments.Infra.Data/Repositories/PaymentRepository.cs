using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Infra.Data.Context;

namespace KRT.Payments.Infra.Data.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentsDbContext _context;

    // Implementação explícita do UnitOfWork
    public IUnitOfWork UnitOfWork => (IUnitOfWork)_context; // Cast seguro pois DbContext implementa IUnitOfWork? Não nativamente.

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
    
    public void Dispose() => _context.Dispose();
}
