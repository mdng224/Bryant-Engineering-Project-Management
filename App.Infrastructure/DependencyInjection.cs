using App.Application.Abstractions.Messaging;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Abstractions.Security;
using App.Infrastructure.Auth;
using App.Infrastructure.Background;
using App.Infrastructure.Email;
using App.Infrastructure.Identity;
using App.Infrastructure.Persistence;
using App.Infrastructure.Persistence.Interceptors;
using App.Infrastructure.Persistence.Readers;
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

        var dataSource = new NpgsqlDataSourceBuilder(connectionString).Build();
        
        services.AddSingleton(dataSource);

        // --- Current user + audit interceptor (per-request) ---
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, HttpContextCurrentUser>();
        services.AddScoped<AuditSaveChangesInterceptor>();
        
        // --- DbContext (single registration) ---
        services.AddDbContext<AppDbContext>((sp, o) =>
        {
            o.UseNpgsql(dataSource, npgsql => npgsql.EnableRetryOnFailure());
            o.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        // --- Repositories / Data access ---
        services.AddScoped<IUserReader, UserReader>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEmployeeReader, EmployeeReader>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IOutboxWriter, OutboxRepository>();
        services.AddScoped<IPositionReader, PositionReader>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IEmployeePositionReader, EmployeePositionReader>();
        services.AddScoped<IClientReader, ClientReader>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IProjectReader, ProjectReader>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IScopeReader, ScopeReader>();
        services.AddScoped<IClientTypeReader, ClientTypeReader>();
        services.AddScoped<IClientCategoryReader, ClientCategoryReader>();
        services.AddScoped<IContactReader, ContactReader>();
        
        // --- Auth helpers (hashing + token creation) ---
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        // --- Email (SMTP + verification) -------------------------------------------
        services.AddOptions<EmailSettings>().Bind(config.GetSection("EmailSettings")).ValidateOnStart();
        services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();

        // --- Background worker ---
        services.AddHostedService<OutboxProcessorWorker>();

        return services;
    }
}
