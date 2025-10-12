using App.Domain.Security;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Data.Configurations;

public sealed class RoleConfig : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> entity)
    {
        entity.ToTable("roles");
        entity.HasKey(r => r.Id);

        // IDs are seeded deterministically, so EF should not try to generate them
        entity.Property(r => r.Id).ValueGeneratedNever();

        entity.Property(r => r.Name)
              .IsRequired()
              .HasMaxLength(100);

        entity.HasIndex(r => r.Name)
              .IsUnique()
              .HasDatabaseName("ux_roles_name");

        // Use anonymous objects for seeding to avoid constructor/setter visibility issues
        entity.HasData(
            new { Id = RoleIds.Administrator, Name = RoleNames.Administrator },
            new { Id = RoleIds.Manager, Name = RoleNames.Manager },
            new { Id = RoleIds.User, Name = RoleNames.User }
        );
    }
}