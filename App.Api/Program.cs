using App.Api.Features.Auth;
using App.Application;
using App.Infrastructure;
using App.Infrastructure.Data;
using App.ServiceDefaults;
using Microsoft.EntityFrameworkCore;

namespace App.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // --- Core & cross-cutting defaults (logging, metrics, etc.)
        builder.AddServiceDefaults();

        // --- Add application + infrastructure layers
        builder.Services.AddAuthorization();
        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        // --- OpenAPI / Swagger setup
        builder.Services.AddOpenApi();      // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

        // --- Database registration (for health checks and data access)
        builder.AddNpgsqlDataSource("appdb");

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var cs = cfg.GetConnectionString("appdb") ?? cfg["Aspire:Npgsql:ConnectionString"];
            Console.WriteLine("DB CS: " + System.Text.RegularExpressions.Regex
                .Replace(cs ?? "", @"(?<=Password=)[^;]+", "****"));
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
        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.Run();
    }
}
