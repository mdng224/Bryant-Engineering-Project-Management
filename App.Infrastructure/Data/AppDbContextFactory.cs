using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace App.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Find solution root dynamically
        var currentDir = Directory.GetCurrentDirectory();
        var solutionDir = currentDir;

        // If running from App.Infrastructure or other project folders,
        // move up until we reach the solution root (where App.Api exists)
        while (!Directory.GetDirectories(solutionDir).Any(d => d.EndsWith("App.Api")) &&
               Directory.GetParent(solutionDir) is DirectoryInfo parent)
        {
            solutionDir = parent.FullName;
        }
        var apiPath = Path.Combine(solutionDir, "App.Api");

        var appApiAssembly = Assembly.Load("App.Api");

        var config = new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets(appApiAssembly, optional: true) // ✅ read App.Api secrets
            .AddEnvironmentVariables()
            .Build();

        // Read from either section
        var connectionString =
            config.GetConnectionString("appdb") ??
            config["Aspire:Npgsql:ConnectionString"];

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}