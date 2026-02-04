using Microsoft.EntityFrameworkCore;
using KRT.BuildingBlocks.Domain;
using KRT.Onboarding.Domain.Entities;

namespace KRT.Onboarding.Infra.Data.Context;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; } // Adicionado

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        return await SaveChangesAsync(cancellationToken);
    }
}
