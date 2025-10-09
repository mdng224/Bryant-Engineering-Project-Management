using App.Api.Contracts.Auth;
using App.Infrastructure;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using App.Application;

namespace App.Api.Extensions;

public static class ServiceCollectionExtensions
{
    private const string CorsPolicy = "frontend";
    private const string AuthScheme = "AppBearer";

    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration cfg)
    {
        // CORS (Vite dev)
        services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
        ));

        // JWT auth
        var keyB64 = cfg["Jwt:KeyBase64"];
        var keyRaw = cfg["Jwt:Key"];
        var keyBytes = !string.IsNullOrWhiteSpace(keyB64)
            ? Convert.FromBase64String(keyB64!)
            : Encoding.UTF8.GetBytes(keyRaw!);

        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = AuthScheme;
                o.DefaultChallengeScheme = AuthScheme;
            })
            .AddJwtBearer(AuthScheme, o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = cfg["Jwt:Issuer"],
                    ValidAudience = cfg["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                    // if your token uses "role" claim, set this:
                    RoleClaimType = ClaimTypes.Role // or "role"
                };
            });

        services.AddAuthorization();

        // OpenAPI
        services.AddOpenApi();

        // App layers
        services.AddApplication();
        services.AddInfrastructure(cfg);

        // Validators
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();

        return services;
    }

    internal static string CorsPolicyName() => CorsPolicy;
}