using App.Domain.Auth;
using App.Domain.Clients;
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
    public DbSet<Client> Clients => Set<Client>();
    
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
        modelBuilder.ApplyConfiguration(new ClientConfig());
    }

    // --- Model configuration -----------------------------------------------
    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Keep domain invariants that are unrelated to auditing
        return ChangeTracker.Entries<User>()
            .Any(e => e.State == EntityState.Modified && e.Property(u => u.Email).IsModified)
            ? throw new InvalidOperationException("Email is immutable after creation.") : base.SaveChangesAsync(ct);
    }
}
