using App.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Data.Configurations;

public sealed class UserConfig : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        entity.ToTable("users");

        // IDs are supplied (Guid v7)
        entity.Property(u => u.Id).ValueGeneratedNever();

        // Email (store as entered), enforce CI uniqueness via normalized shadow column
        entity.Property(u => u.Email).IsRequired().HasMaxLength(320);

        // Normalized email (lowercased) – stored generated column
        entity.Property<string>("EmailNormalized")
              .HasColumnType("text")
              .HasComputedColumnSql("lower(\"Email\")", stored: true);

        // Unique for non-deleted users only (allows reuse after soft delete)
        entity.HasIndex("EmailNormalized")
              .HasDatabaseName("ux_users_email_normalized")
              .IsUnique()
              .HasFilter("\"DeletedAtUtc\" IS NULL"); // Postgres filter syntax

        entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);

        // --- Single role per user -------------------------------------------
        entity.HasOne(u => u.Role)
              .WithMany(r => r.Users)
              .HasForeignKey(u => u.RoleId)
              .OnDelete(DeleteBehavior.Restrict)   // don’t delete users if a role is removed
              .IsRequired();

        entity.HasIndex(u => u.RoleId).HasDatabaseName("ix_users_role_id");

        // --- Auditing --------------------------------------------------------
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