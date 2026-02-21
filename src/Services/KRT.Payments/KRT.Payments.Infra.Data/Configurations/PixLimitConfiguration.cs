using KRT.Payments.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KRT.Payments.Infra.Data.Configurations;

public class PixLimitConfiguration : IEntityTypeConfiguration<PixLimit>
{
    public void Configure(EntityTypeBuilder<PixLimit> builder)
    {
        builder.ToTable("PixLimits");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.AccountId).IsUnique();
        builder.Property(x => x.DaytimePerTransaction).HasPrecision(18, 2);
        builder.Property(x => x.DaytimeDaily).HasPrecision(18, 2);
        builder.Property(x => x.NighttimePerTransaction).HasPrecision(18, 2);
        builder.Property(x => x.NighttimeDaily).HasPrecision(18, 2);
        builder.Property(x => x.DaytimeUsedToday).HasPrecision(18, 2);
        builder.Property(x => x.NighttimeUsedToday).HasPrecision(18, 2);
    }
}