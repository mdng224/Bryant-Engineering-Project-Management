using App.Domain.Auth;
using App.Domain.Common;
using App.Domain.Employees;
using App.Domain.Users;
using App.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // --- Events  -------------------------------------------------
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<EmailVerification> EmailVerifications => Set<EmailVerification>();
    
    // --- Security -------------------------------------------------
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();

    // --- Employees / HR -------------------------------------------
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<EmployeePosition> EmployeePositions => Set<EmployeePosition>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Reference order matters
        modelBuilder.ApplyConfiguration(new OutboxMessageConfig());     // 0
        modelBuilder.ApplyConfiguration(new RoleConfig());             // 1
        modelBuilder.ApplyConfiguration(new UserConfig());             // 2
        modelBuilder.ApplyConfiguration(new PositionConfig());         // 3
        modelBuilder.ApplyConfiguration(new EmployeeConfig());         // 4
        modelBuilder.ApplyConfiguration(new EmployeePositionConfig()); // 5
        modelBuilder.ApplyConfiguration(new EmailVerificationConfig()); // 6
        // modelBuilder.ApplyConfiguration(new ClientConfig());        // 
    }

    // --- Model configuration -----------------------------------------------
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).CurrentValue = now;
                    entry.Property(nameof(IAuditableEntity.UpdatedAtUtc)).CurrentValue = now;
                    break;

                case EntityState.Modified:
                    // Prevent overwriting CreatedAtUtc on updates
                    entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).IsModified = false;
                    entry.Property(nameof(IAuditableEntity.UpdatedAtUtc)).CurrentValue = now;
                    break;

                case EntityState.Deleted:
                    // Soft delete: mark as modified and set DeletedAtUtc
                    entry.State = EntityState.Modified;
                    entry.Property(nameof(IAuditableEntity.DeletedAtUtc)).CurrentValue = now;
                    entry.Property(nameof(IAuditableEntity.UpdatedAtUtc)).CurrentValue = now;
                    break;

                case EntityState.Detached:
                case EntityState.Unchanged:
                default:
                    break;  // no-op
            }
        }

        // Enforce immutable email invariant
        return ChangeTracker.Entries<User>()
            .Any(ee => ee.State == EntityState.Modified && ee.Property(u => u.Email) .IsModified)
            ? throw new InvalidOperationException("Email is immutable after creation.")
            : base.SaveChangesAsync(ct);
    }
}
