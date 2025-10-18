using App.Domain.Security;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Persistence.Configurations;

public sealed class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> b)
    {
        // --- Table  ----------------------------------------------------
        b.ToTable("roles");
        
        // --- Key -------------------------------------------------------
        b.HasKey(r => r.Id);
        
        // --- Properties -----------------------------------------------------
        b.Property(r => r.Id).ValueGeneratedNever();   // IDs are seeded deterministically, so EF should not try to generate them                     
        b.Property(r => r.Name).IsRequired().HasMaxLength(100);
        b.HasIndex(r => r.Name).IsUnique().HasDatabaseName("ux_roles_name");

        // --- Seed Data ------------------------------------------------------
        b.HasData(
            new { Id = RoleIds.Administrator, Name = RoleNames.Administrator },
            new { Id = RoleIds.Manager,       Name = RoleNames.Manager },
            new { Id = RoleIds.User,          Name = RoleNames.User }
        );
    }
}