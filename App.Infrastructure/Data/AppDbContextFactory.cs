using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace App.Infrastructure.Data;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var apiPath = GetApiPath();
        var config = BuildConfiguration(apiPath);
        var options = BuildOptions(config);

        return new AppDbContext(options);
    }

    // --- Helpers -------------------------------------------------------------

    private static string GetApiPath()
    {
        // Walk up from the current directory until we find a folder that contains App.Api
        var dir = Directory.GetCurrentDirectory();

        while (dir is not null &&
               !Directory.GetDirectories(dir).Any(d => d.EndsWith("App.Api", StringComparison.OrdinalIgnoreCase)))
        {
            dir = Directory.GetParent(dir)?.FullName;
        }

        if (dir is null)
            throw new InvalidOperationException("Could not locate solution root containing 'App.Api'.");

        return Path.Combine(dir, "App.Api");
    }

    private static IConfiguration BuildConfiguration(string apiPath)
    {
        // Load App.Api assembly so user-secrets for that project are available
        var appApiAssembly = Assembly.Load(new AssemblyName("App.Api"));

        return new ConfigurationBuilder()
            .SetBasePath(apiPath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets(appApiAssembly, optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    private static DbContextOptions<AppDbContext> BuildOptions(IConfiguration config)
    {
        var connectionString =
              config.GetConnectionString("appdb")
           ?? config["Aspire:Npgsql:ConnectionString"]
           ?? throw new InvalidOperationException(
                "No connection string found. Provide ConnectionStrings:appdb or Aspire:Npgsql:ConnectionString.");

        return new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }
}