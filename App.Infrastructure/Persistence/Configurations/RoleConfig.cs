using App.Domain.Security;
using App.Domain.Users;
using App.Infrastructure.Persistence.Seed;
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
        
        // --- Properties ------------------------------------------------
        b.Property(r => r.Id)
            .HasColumnName("id")
            .ValueGeneratedNever(); // deterministic seed IDs 
        
        b.Property(r => r.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(100);
        
        b.HasIndex(r => r.Name)
            .IsUnique()
            .HasDatabaseName("ux_roles_name");
    }
}