using KRT.BuildingBlocks.Domain;
using KRT.BuildingBlocks.Infrastructure.Outbox;
using KRT.Onboarding.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace KRT.Onboarding.Infra.Data.Context;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    private readonly IMediator _mediator;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IMediator mediator) 
        : base(options)
    {
        _mediator = mediator;
    }

    public DbSet<Account> Accounts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // CORREÇÃO: Ignorar DomainEvent — ele NÃO é uma tabela
        modelBuilder.Ignore<DomainEvent>();

        // Configuração da entidade Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.CustomerName).IsRequired().HasMaxLength(100);
            entity.Property(a => a.Document).IsRequired().HasMaxLength(14);
            entity.Property(a => a.Email).IsRequired().HasMaxLength(150);
            entity.Property(a => a.Balance).HasPrecision(18, 2);
            entity.Property(a => a.RowVersion).IsRowVersion();
        });

        base.OnModelCreating(modelBuilder);
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        await DispatchDomainEventsAsync(ct);
        return await base.SaveChangesAsync(ct);
    }

    private async Task DispatchDomainEventsAsync(CancellationToken ct)
    {
        var domainEntities = ChangeTracker
            .Entries<Entity>()
            .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();

        domainEntities.ToList()
            .ForEach(entity => entity.Entity.ClearDomainEvents());

        foreach (var domainEvent in domainEvents)
            await _mediator.Publish(domainEvent, ct);
    }
}
