using App.Domain.Auth;
using App.Domain.Clients;
using App.Domain.Common;
using App.Domain.Employees;
using App.Domain.Projects;
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
    
    // --- Clients / Projects --------------------------------------------------
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ClientCategory> ClientCategories => Set<ClientCategory>();
    public DbSet<ClientType> ClientTypes => Set<ClientType>();
    public DbSet<Scope> Scopes => Set<Scope>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Reference order matters
        modelBuilder.ApplyConfiguration(new OutboxMessageConfig());
        modelBuilder.ApplyConfiguration(new RoleConfig());
        modelBuilder.ApplyConfiguration(new UserConfig());
        modelBuilder.ApplyConfiguration(new PositionConfig());
        modelBuilder.ApplyConfiguration(new EmployeeConfig());
        modelBuilder.ApplyConfiguration(new EmployeePositionConfig());
        modelBuilder.ApplyConfiguration(new EmailVerificationConfig());
        modelBuilder.ApplyConfiguration(new ClientCategoryConfig());
        modelBuilder.ApplyConfiguration(new ClientTypeConfig());
        modelBuilder.ApplyConfiguration(new ClientConfig());
        modelBuilder.ApplyConfiguration(new ScopeConfig());
        modelBuilder.ApplyConfiguration(new ProjectConfig());
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
