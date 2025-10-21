using App.Application.Abstractions;
using App.Infrastructure.Auth;
using App.Infrastructure.Persistence;
using App.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace App.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // --- DbContext ---
        var connectionString =
            config.GetConnectionString("appdb")
            ?? config["Aspire:Npgsql:ConnectionString"]
            ?? throw new InvalidOperationException(
                "No connection string found. Set ConnectionStrings:appdb or Aspire:Npgsql:ConnectionString.");

        services.AddDbContextPool<AppDbContext>(o =>
        {
            o.UseNpgsql(connectionString, npgsql => npgsql.EnableRetryOnFailure());
        });

        // --- Repositories / Data access ---
        services.AddScoped<IUserReader, UserRepository>();
        services.AddScoped<IUserWriter, UserRepository>();
        services.AddScoped<IOutboxWriter, OutboxRepository>();

        // --- Auth helpers (hashing + token creation) ---
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // ⛔️ Do NOT register Authentication/Authorization here.
        // Those belong in the API layer (AddApi) so you avoid double-registration
        // and keep Infra provider-agnostic.

        // (optional) Npgsql data source for health checks if you use it elsewhere
        services.AddSingleton(_ => new NpgsqlDataSourceBuilder(connectionString).Build());

        return services;
    }
}
