using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;
using KRT.Payments.Domain.Interfaces;
using KRT.Payments.Infra.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace KRT.Payments.Infra.Data.Repositories;

/// <summary>
/// DEPRECATED: Use PixTransactionRepository + OnboardingServiceClient para Saga.
/// Mantido apenas para compatibilidade de compilação.
/// </summary>
public class PaymentRepository : IPaymentRepository
{
    private readonly PaymentsDbContext _context;
    public IUnitOfWork UnitOfWork => _context;
    public PaymentRepository(PaymentsDbContext context) => _context = context;

    public Task AddTransactionAsync(PixTransaction transaction)
        => _context.PixTransactions.AddAsync(transaction).AsTask();

    public Task<KRT.Payments.Domain.Models.AccountView?> GetAccountViewAsync(Guid accountId)
        => Task.FromResult<KRT.Payments.Domain.Models.AccountView?>(null);
}
