using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;

namespace KRT.Payments.Infra.Data.Context;

public class PaymentsDbContext : DbContext, IUnitOfWork
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<PixTransaction> PixTransactions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<PixTransaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.PixKey).HasMaxLength(100);
            e.Property(x => x.Currency).HasMaxLength(3).HasDefaultValue("BRL");
            e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.FailureReason).HasMaxLength(500);

            e.HasIndex(x => x.IdempotencyKey).IsUnique();
            e.HasIndex(x => x.SourceAccountId);
            e.HasIndex(x => x.DestinationAccountId);
        });

        base.OnModelCreating(modelBuilder);
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }
}
