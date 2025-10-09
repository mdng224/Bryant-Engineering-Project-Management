using App.Api.Features.Auth;
using App.Application;
using App.Application.Auth;
using App.Infrastructure;
using App.Infrastructure.Data;
using App.ServiceDefaults;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace App.Api;

public class Program
{
    private const string AuthScheme = "AppBearer";   // <— custom scheme name
    private const string CorsPolicy = "frontend";

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- Core & cross-cutting defaults (logging, metrics, etc.)
        builder.AddServiceDefaults();

        // --- Web API specifics (controllers, JSON options, API explorer)
        builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
            .WithOrigins("http://localhost:5173") // Vite dev origin
            .AllowAnyHeader()
            .AllowAnyMethod()
        // add .AllowCredentials() only if you’ll use cookies
        ));

        // JWT
        var keyB64 = builder.Configuration["Jwt:KeyBase64"];
        var keyRaw = builder.Configuration["Jwt:Key"];
        var keyBytes = !string.IsNullOrWhiteSpace(keyB64)
            ? Convert.FromBase64String(keyB64)
            : Encoding.UTF8.GetBytes(keyRaw!);
        builder.Services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = AuthScheme;
                options.DefaultChallengeScheme = AuthScheme;
            })
            .AddJwtBearer(AuthScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
                };
            });

        // --- Add application + infrastructure layers
        builder.Services.AddAuthorization();
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);
        builder.Services.AddValidatorsFromAssembly(typeof(LoginDtoValidator).Assembly);

        // --- OpenAPI / Swagger setup
        builder.Services.AddOpenApi();      // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

        // --- Database registration (for health checks and data access)
        builder.AddNpgsqlDataSource("appdb");

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }

        // --- Health check endpoint
        app.MapGet("/health/db", async (Npgsql.NpgsqlDataSource ds) =>
        {
            await using var cmd = ds.CreateCommand("SELECT 1");
            var result = await cmd.ExecuteScalarAsync();
            return Results.Ok(new { db = result });
        });

        // --- Feature endpoints
        app.MapDefaultEndpoints();
        app.MapAuthEndpoints();

        // --- Development-only tools (Swagger/OpenAPI)
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/openapi/v1.json", "App.Api v1");
                o.SwaggerEndpoint("/openapi/Authentication.json", "Authentication");
            });
        }

        // --- Middleware pipeline
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseCors(CorsPolicy);
        app.UseAuthentication();
        app.UseAuthorization();

        app.Run();
    }
}
