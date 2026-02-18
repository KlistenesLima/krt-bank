using Microsoft.EntityFrameworkCore;
using KRT.Payments.Domain.Entities;
using KRT.BuildingBlocks.Domain;
using KRT.Payments.Api.Controllers;

namespace KRT.Payments.Api.Data;

public class PaymentsDbContext : DbContext
{
    public PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : base(options) { }

    // === Entidades ja existentes ===
    public DbSet<PixTransaction> PixTransactions => Set<PixTransaction>();
    public DbSet<Boleto> Boletos => Set<Boleto>();
    public DbSet<PixContact> PixContacts => Set<PixContact>();
    public DbSet<FinancialGoal> FinancialGoals => Set<FinancialGoal>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<StatementEntry> StatementEntries => Set<StatementEntry>();
    public DbSet<PixLimit> PixLimits => Set<PixLimit>();
    public DbSet<ScheduledPix> ScheduledPixTransactions => Set<ScheduledPix>();
    public DbSet<VirtualCard> VirtualCards => Set<VirtualCard>();
    public DbSet<PixCharge> PixCharges => Set<PixCharge>();

    // === Entidades migradas (antes ConcurrentDictionary) ===
    public DbSet<InsurancePolicy> InsurancePolicies => Set<InsurancePolicy>();
    public DbSet<KycProfile> KycProfiles => Set<KycProfile>();
    public DbSet<UserPoints> UserPointsTable => Set<UserPoints>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Ignore<DomainEvent>();

        // PixTransaction
        modelBuilder.Entity<PixTransaction>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.SourceAccountId);
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => x.DestinationAccountId);
        });

        // Boleto
        modelBuilder.Entity<Boleto>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AccountId);
            e.HasIndex(x => x.Barcode).IsUnique();
        });

        // PixContact
        modelBuilder.Entity<PixContact>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AccountId);
        });

        // FinancialGoal
        modelBuilder.Entity<FinancialGoal>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AccountId);
        });

        // Notification
        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.AccountId, x.IsRead });
        });

        // StatementEntry
        modelBuilder.Entity<StatementEntry>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.AccountId, x.Date });
        });

        // PixLimit
        modelBuilder.Entity<PixLimit>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AccountId).IsUnique();
        });

        // ScheduledPix
        modelBuilder.Entity<ScheduledPix>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AccountId);
        });

        // VirtualCard
        modelBuilder.Entity<VirtualCard>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AccountId);
        });

        // InsurancePolicy
        modelBuilder.Entity<InsurancePolicy>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AccountId);
        });

        // KycProfile
        modelBuilder.Entity<KycProfile>(e =>
        {
            e.HasKey(x => x.AccountId);
            e.OwnsOne(x => x.DocumentValidation);
            e.OwnsOne(x => x.FaceMatch);
        });

        // UserPoints
        modelBuilder.Entity<UserPoints>(e =>
        {
            e.HasKey(x => x.AccountId);
        });

        // UserProfile
        modelBuilder.Entity<UserProfile>(e =>
        {
            e.HasKey(x => x.AccountId);
            e.OwnsOne(x => x.Address);
            e.OwnsOne(x => x.Preferences);
            e.OwnsOne(x => x.Security);
        });

        // PixCharge
        modelBuilder.Entity<PixCharge>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ExternalId);
            e.Property(x => x.Amount).HasPrecision(18, 2);
            e.Property(x => x.Status).HasConversion<string>();
        });
    }
}


