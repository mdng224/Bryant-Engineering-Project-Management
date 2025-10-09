using App.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Data.Configurations;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");

        entity.Property(u => u.Id)
              .ValueGeneratedNever(); // supplying Guid v7 in code

        // Email as case-insensitive text
        entity.Property(u => u.Email)
              .IsRequired()
              .HasMaxLength(255);

        // Normalized email (lowercased) – stored generated column
        entity.Property<string>("EmailNormalized")
              .HasColumnType("text")
              .HasComputedColumnSql("lower(\"Email\")", stored: true);

        // Enforce case-insensitive uniqueness via the normalized value
        entity.HasIndex("EmailNormalized")
              .HasDatabaseName("ux_users_email_normalized")
              .IsUnique();

        entity.Property(u => u.PasswordHash)
              .IsRequired()
              .HasMaxLength(60); // typical BCrypt length

        entity.Property(u => u.CreatedAtUtc)
              .IsRequired()
              .HasColumnType("timestamptz")
              .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.Property(u => u.UpdatedAtUtc)
              .IsRequired()
              .HasColumnType("timestamptz");
    }
}