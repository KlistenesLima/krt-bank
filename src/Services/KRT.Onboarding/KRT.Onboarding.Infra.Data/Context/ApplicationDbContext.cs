using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.Infrastructure.Outbox;
using KRT.Onboarding.Domain.Entities;
using FluentValidation.Results;

namespace KRT.Onboarding.Infra.Data.Context;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<DomainEvent>();
        modelBuilder.Ignore<ValidationResult>();
        
        // Configuração do OutboxMessage
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("OutboxMessages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.OccurredOn).IsRequired();
            entity.Property(e => e.Error).HasMaxLength(2000);
            entity.Property(e => e.CorrelationId).HasMaxLength(100);
            entity.Property(e => e.CausationId).HasMaxLength(100);
            entity.HasIndex(e => e.ProcessedOn);
        });
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
    
    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }
}
