using KRT.Onboarding.Domain.Entities;
using KRT.Onboarding.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KRT.Onboarding.Infra.Data.Mappings;

public class PixKeyMapping : IEntityTypeConfiguration<PixKey>
{
    public void Configure(EntityTypeBuilder<PixKey> builder)
    {
        builder.ToTable("pix_keys");

        builder.HasKey(pk => pk.Id);

        builder.Property(pk => pk.Id)
            .HasColumnName("id");

        builder.Property(pk => pk.AccountId)
            .HasColumnName("account_id")
            .IsRequired();

        builder.Property(pk => pk.KeyType)
            .HasColumnName("key_type")
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(pk => pk.KeyValue)
            .HasColumnName("key_value")
            .HasMaxLength(77)
            .IsRequired();

        builder.Property(pk => pk.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(pk => pk.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(pk => pk.DeactivatedAt)
            .HasColumnName("deactivated_at");

        // Unique index: garante chave ativa unica
        builder.HasIndex(pk => new { pk.KeyType, pk.KeyValue })
            .HasDatabaseName("ix_pix_keys_type_value")
            .HasFilter("is_active = true")
            .IsUnique();

        // Index para busca por conta
        builder.HasIndex(pk => pk.AccountId)
            .HasDatabaseName("ix_pix_keys_account_id");

        // Relationship
        builder.HasOne(pk => pk.Account)
            .WithMany()
            .HasForeignKey(pk => pk.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
