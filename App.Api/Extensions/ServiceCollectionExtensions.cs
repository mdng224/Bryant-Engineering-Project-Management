// App.Infrastructure/Auth/JwtTokenService.cs
using App.Api.Contracts.Admins;
using App.Api.Contracts.Auth;
using App.Application;
using App.Domain.Security;
using App.Infrastructure;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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

        // --- JWT (symmetric) -------------------------------------------------
        var keyB64 = cfg["Jwt:KeyBase64"];
        var keyRaw = cfg["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(keyB64) && string.IsNullOrWhiteSpace(keyRaw))
            throw new InvalidOperationException("JWT key not configured. Provide Jwt:KeyBase64 or Jwt:Key.");

        var keyBytes = !string.IsNullOrWhiteSpace(keyB64)
            ? Convert.FromBase64String(keyB64!)
            : Encoding.UTF8.GetBytes(keyRaw!);

        // IMPORTANT: do not map inbound claims; keep "role" as "role"
        JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

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

                    // Match the claim type you emit when creating tokens:
                    // new Claim(ClaimTypes.Role, RoleNames.Administrator)
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.Zero // Avoid hidden 5-minute leniency during tests
                };
            });

        // --- Authorization policies ------------------------------------------
        services.AddAuthorizationBuilder()
            .AddPolicy("AdminOnly", p => p.RequireRole(RoleNames.Administrator));

        // --- OpenAPI & app layers --------------------------------------------
        services.AddOpenApi(options =>
        {
            // register by type
            options.AddDocumentTransformer<JwtSecuritySchemeTransformer>();
        });
        services.AddApplication();
        services.AddInfrastructure(cfg);

        // --- Validators -------------------------------------------------------
        services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
        services.AddScoped<IValidator<RegisterRequest>, RegisterRequestValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, SetUserRoleRequestValidator>();
        services.AddScoped<IValidator<UpdateUserRequest>, UpdateUserRequestValidator>();

        return services;
    }

    internal static string CorsPolicyName() => CorsPolicy;
}