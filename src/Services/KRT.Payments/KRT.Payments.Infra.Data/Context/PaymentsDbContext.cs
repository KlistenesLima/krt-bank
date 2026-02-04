using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;

namespace KRT.Payments.Infra.Data.Context;

public class PaymentsDbContext : DbContext, IUnitOfWork
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }
    
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<Payment>().HasKey(p => p.Id);
        modelBuilder.Entity<Payment>().Property(p => p.Amount).HasPrecision(18, 2);
    }

    // CORREÇÃO: Assinatura exata da interface (int + CancellationToken)
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }
}
