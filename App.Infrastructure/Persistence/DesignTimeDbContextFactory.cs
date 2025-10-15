using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace App.Infrastructure.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Load env + user-secrets. We want the SAME secret the AppHost uses.
        var appHostAssembly = Assembly.Load("App.AppHost"); // 👈 adjust if yours is "AppHost"
        var cfg = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets(appHostAssembly, optional: true)
            .Build();

        var pw = cfg["parameters:postgres-password"]
                 ?? throw new InvalidOperationException("Missing user secret: parameters:postgres-password");

        var cs = $"Host=localhost;Port=55432;Database=appdb;Username=postgres;Password={pw}";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new AppDbContext(options);
    }
}