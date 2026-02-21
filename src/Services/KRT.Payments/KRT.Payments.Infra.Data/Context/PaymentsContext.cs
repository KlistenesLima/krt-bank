using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.Infrastructure.Outbox;
using KRT.Payments.Domain.Entities;

namespace KRT.Payments.Infra.Data.Context;

public class PaymentsDbContext : DbContext, IUnitOfWork
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    public DbSet<PixTransaction> PixTransactions { get; set; } = null!;
    public DbSet<PixLimit> PixLimits { get; set; } = null!;
    public DbSet<VirtualCard> VirtualCards { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fraud Analysis columns
        modelBuilder.Entity<PixTransaction>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasIndex(e => e.IdempotencyKey).IsUnique();
            entity.HasIndex(e => e.SourceAccountId);
            entity.HasIndex(e => e.DestinationAccountId);
            entity.Property(e => e.FraudScore).IsRequired(false);
            entity.Property(e => e.FraudDetails).HasMaxLength(1000).IsRequired(false);
            entity.Property(e => e.FraudAnalyzedAt).IsRequired(false);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => new { e.ProcessedOn, e.RetryCount });
        });

        modelBuilder.ApplyConfiguration(new Configurations.PixLimitConfiguration());
        modelBuilder.ApplyConfiguration(new Configurations.VirtualCardConfiguration());

        modelBuilder.Ignore<DomainEvent>();
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        return await SaveChangesAsync(ct);
    }
}