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
        b.Property(u => u.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // IDs are supplied (Guid v7)
        
        b.Property(u => u.Email)
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(128);
        
        b.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .IsRequired()
            .HasMaxLength(255);
        
        b.Property(u => u.IsActive)
            .HasColumnName("is_active")
            .IsRequired()
            .HasDefaultValue(false); // Users are inactive by default until explicitly activated.
        
        b.Property(u => u.RoleId)
            .HasColumnName("role_id");
        
        b.Property(u => u.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .IsRequired()
            .HasColumnType("timestamptz")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        
        b.Property(u => u.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .IsRequired()
            .HasColumnType("timestamptz");
        
        b.Property(u => u.DeletedAtUtc)
            .HasColumnName("deleted_at_utc")
            .HasColumnType("timestamptz");
        
        // --- Relationships --------------------------------------------------
        b.HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
        
        // --- Indexes / Uniqueness ------------------------------------------
        b.HasIndex(u => u.RoleId)
            .HasDatabaseName("ix_users_role_id");

        b.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ux_users_email")
            .HasFilter("deleted_at_utc IS NULL");
    }
}