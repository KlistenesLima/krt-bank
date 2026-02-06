using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.Payments.Domain.Entities;

namespace KRT.Payments.Infra.Data.Context;

public class PaymentsDbContext : DbContext, IUnitOfWork
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<PixTransaction> PixTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).HasPrecision(18, 2);
            entity.Property(p => p.ReceiverKey).IsRequired().HasMaxLength(100);
        });

        // PixTransaction
        modelBuilder.Entity<PixTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Amount).HasPrecision(18, 2);
            entity.Property(t => t.PixKey).IsRequired().HasMaxLength(100);
            entity.Property(t => t.Currency).HasMaxLength(3).HasDefaultValue("BRL");
            entity.Property(t => t.Description).HasMaxLength(200);
            entity.Property(t => t.FailureReason).HasMaxLength(500);

            // Index para idempotencia
            entity.HasIndex(t => t.IdempotencyKey).IsUnique();

            // Index para busca por conta
            entity.HasIndex(t => t.SourceAccountId);
            entity.HasIndex(t => t.DestinationAccountId);
        });
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }
}
