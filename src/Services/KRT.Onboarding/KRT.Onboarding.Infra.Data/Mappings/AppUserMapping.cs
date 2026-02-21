using KRT.Onboarding.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KRT.Onboarding.Infra.Data.Mappings;

public class AppUserMapping : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable("AppUsers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Document).IsRequired().HasMaxLength(14);
        builder.Property(x => x.PasswordHash).IsRequired();
        builder.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.EmailConfirmationCode).HasMaxLength(10);
        builder.Property(x => x.PasswordResetCode).HasMaxLength(10);
        builder.Property(x => x.KeycloakUserId).HasMaxLength(100);
        builder.Property(x => x.ApprovedBy).HasMaxLength(200);

        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.Document).IsUnique();
    }
}
