using KRT.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KRT.Payments.Infra.Data.Configurations;

public class VirtualCardConfiguration : IEntityTypeConfiguration<VirtualCard>
{
    public void Configure(EntityTypeBuilder<VirtualCard> builder)
    {
        builder.ToTable("VirtualCards");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => x.CardNumber).IsUnique();
        builder.Property(x => x.CardNumber).HasMaxLength(16);
        builder.Property(x => x.CardholderName).HasMaxLength(100);
        builder.Property(x => x.Cvv).HasMaxLength(3);
        builder.Property(x => x.Last4Digits).HasMaxLength(4);
        builder.Property(x => x.SpendingLimit).HasPrecision(18, 2);
        builder.Property(x => x.SpentThisMonth).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>();
        builder.Property(x => x.Brand).HasConversion<string>();
    }
}