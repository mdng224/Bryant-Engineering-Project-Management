using App.Domain.Common;
using App.Domain.Users;
using App.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfig());
        modelBuilder.ApplyConfiguration(new RoleConfig());
        modelBuilder.ApplyConfiguration(new EmployeeConfig());
        modelBuilder.ApplyConfiguration(new ClientConfig());
    }

    /* Even though individual entities (e.g., User, Client) set timestamps and expose
     intentful methods like Touch() and SoftDelete(), this override is kept as a
     defensive, last-line-of-defense layer so invariants still hold when:
     Changes are applied directly to tracked entities (e.g., from a mapper/DTO) without calling domain methods.*/
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Deleted)
            {
                // Global soft-delete
                entry.State = EntityState.Modified;
                entry.Property(nameof(IAuditableEntity.DeletedAtUtc)).CurrentValue = now;
                entry.Property(nameof(IAuditableEntity.UpdatedAtUtc)).CurrentValue = now;
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                // Set once on create
                entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).CurrentValue = now;
                entry.Property(nameof(IAuditableEntity.UpdatedAtUtc)).CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).IsModified = false;
                entry.Property(nameof(IAuditableEntity.UpdatedAtUtc)).CurrentValue = now;
            }
        }

        foreach (var e in ChangeTracker.Entries<User>())
        {
            if (e.State == EntityState.Modified && e.Property(u => u.Email).IsModified)
                throw new InvalidOperationException("Email is immutable after creation.");
        }

        return base.SaveChangesAsync(ct);
    }
}
