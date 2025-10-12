using App.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Data.Configurations;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");

        // --- Keys ------------------------------------------------------------
        entity.Property(u => u.Id).ValueGeneratedNever();   // IDs are supplied (Guid v7)

        // --- Email -----------------------------------------------------------
        entity.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(128);                             // Almost all modern providers reject anything longer than 128 chars.
        entity.HasIndex(u => u.Email)                       //  (case sensitivity handled by the app layer)
              .HasDatabaseName("ux_users_email")
              .IsUnique()
              .HasFilter("\"DeletedAtUtc\" IS NULL");

        // --- Password -------------------------------------------------------
        entity.Property(u => u.PasswordHash)
              .IsRequired()
              .HasMaxLength(60);                            // BCrypt hashes are 60 characters.

        // --- Relationships --------------------------------------------------
        entity.HasOne(u => u.Role)
              .WithMany(r => r.Users)
              .HasForeignKey(u => u.RoleId)
              .OnDelete(DeleteBehavior.Restrict)
              .IsRequired();

        entity.HasIndex(u => u.RoleId).HasDatabaseName("ix_users_role_id");

        // --- IsActive -------------------------------------------------------
        entity.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(false);                        // Users are inactive by default until explicitly activated.

        // --- Auditing -------------------------------------------------------
        entity.Property(u => u.CreatedAtUtc)
              .IsRequired()
              .HasColumnType("timestamptz")
              .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.Property(u => u.UpdatedAtUtc)
              .IsRequired()
              .HasColumnType("timestamptz")
              .HasDefaultValueSql("CURRENT_TIMESTAMP");

        entity.Property(u => u.DeletedAtUtc).HasColumnType("timestamptz");
    }
}