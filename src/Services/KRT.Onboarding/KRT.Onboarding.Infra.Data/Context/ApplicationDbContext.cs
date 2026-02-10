using KRT.Onboarding.Infra.Data.Mappings;
using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.Infrastructure.Outbox;
using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;

namespace KRT.Onboarding.Infra.Data.Context;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<PixKey> PixKeys => Set<PixKey>();
    public DbSet<Account> Accounts { get; set; } = null!;
    public DbSet<OutboxMessage> OutboxMessages { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CustomerName).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Document).HasMaxLength(14).IsRequired();
            entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Balance).HasPrecision(18, 2);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Type).HasConversion<string>();
            entity.HasIndex(e => e.Document).IsUnique();
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasMaxLength(500).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.HasIndex(e => new { e.ProcessedOn, e.RetryCount });
        });
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }
}


