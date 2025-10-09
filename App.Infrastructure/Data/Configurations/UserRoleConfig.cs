using App.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace App.Infrastructure.Data.Configurations;

public sealed class UserRoleConfig : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> entity)
    {
        entity.ToTable("user_roles");

        entity.HasKey(x => new { x.UserId, x.RoleId }); // composite PK

        entity.HasOne(x => x.User)
              .WithMany(u => u.UserRoles)
              .HasForeignKey(x => x.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        entity.HasOne(x => x.Role)
              .WithMany(r => r.UserRoles)
              .HasForeignKey(x => x.RoleId)
              .OnDelete(DeleteBehavior.Cascade);

        // helpful index to find users by role quickly
        entity.HasIndex(x => x.RoleId).HasDatabaseName("ix_user_roles_role");
    }
}