using App.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        // --- Table -----------------------------------------------------------------
        b.ToTable("users");

        // --- Keys ------------------------------------------------------------
        b.HasKey(u => u.Id);

        // --- Properties -----------------------------------------------------
        b.Property(u => u.Id).ValueGeneratedNever(); // IDs are supplied (Guid v7)
        b.Property(u => u.Email).IsRequired().HasMaxLength(128);
        b.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
        b.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(false); // Users are inactive by default until explicitly activated.
        b.Property(u => u.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        b.Property(u => u.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("timestamptz");
        b.Property(u => u.DeletedAtUtc).HasColumnType("timestamptz");
        
        // --- Relationships --------------------------------------------------
        b.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        // --- Indexes / Uniqueness ------------------------------------------
        b.HasIndex(u => u.RoleId).HasDatabaseName("ix_users_role_id");
        b.HasIndex(u => u.Email).HasDatabaseName("ux_users_email").IsUnique().HasFilter("\"DeletedAtUtc\" IS NULL");
    }
}