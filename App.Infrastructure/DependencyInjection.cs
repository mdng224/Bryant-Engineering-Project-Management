using App.Application.Abstractions;
using App.Application.Abstractions.Messaging;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Infrastructure.Auth;
using App.Infrastructure.Background;
using App.Infrastructure.Email;
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
        // --- Connection / DataSource ---
        var connectionString =
            config.GetConnectionString("appdb")
            ?? config["Aspire:Npgsql:ConnectionString"]
            ?? throw new InvalidOperationException(
                "No connection string found. Set ConnectionStrings:appdb or Aspire:Npgsql:ConnectionString.");

        // Build ONE shared pool/data source and reuse it everywhere
        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        services.AddSingleton(dataSource);

        // --- DbContext (single registration) ---
        services.AddDbContextPool<AppDbContext>(o =>
        {
            o.UseNpgsql(dataSource, npgsql => npgsql.EnableRetryOnFailure());
            // Optional: o.UseSnakeCaseNamingConvention();
        });

        // --- Repositories / Data access ---
        services.AddScoped<IUserReader, UserRepository>();
        services.AddScoped<IUserWriter, UserRepository>();
        services.AddScoped<IEmployeeReader, EmployeeRepository>();
        services.AddScoped<IOutboxWriter, OutboxRepository>();
        services.AddScoped<IPositionReader, PositionRepository>();
        services.AddScoped<IPositionWriter, PositionRepository>();
        services.AddScoped<IEmployeePositionReader, EmployeePositionRepository>();
        
        // --- Auth helpers (hashing + token creation) ---
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // --- Email (SMTP + verification) -------------------------------------------
        services.AddOptions<EmailSettings>().Bind(config.GetSection("EmailSettings")).ValidateOnStart();
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IEmailVerificationReader, EmailVerificationRepository>();
        services.AddScoped<IEmailVerificationWriter, EmailVerificationRepository>();

        // --- Background worker ---
        services.AddHostedService<OutboxProcessorWorker>();

        return services;
    }
}
