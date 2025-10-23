using App.Api.Features.Auth;
using App.Api.Features.Admins;
using App.Infrastructure.Persistence;
using App.Infrastructure.Persistence.Seed;
using App.ServiceDefaults;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace App.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApi(this WebApplication app)
    {
        // dev-only swagger
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/openapi/v1.json", "App.Api v1");
                o.SwaggerEndpoint("/openapi/Authentication.json", "Authentication");
            });
        }
        else
        {
            app.UseHttpsRedirection();
        }

        // pipeline
        app.UseCors(ServiceCollectionExtensions.CorsPolicyName());
        app.UseAuthentication();
        app.UseAuthorization();

        // health
        app.MapGet("/health/db", async (NpgsqlDataSource ds) =>
        {
            await using var cmd = ds.CreateCommand("SELECT 1");
            var result = await cmd.ExecuteScalarAsync();
            return Results.Ok(new { db = result });
        });

        // feature endpoints
        app.MapDefaultEndpoints();
        app.MapAdminEndpoints();
        app.MapAuthEndpoints();
        app.MapVerificationEndpoints();
        
        return app;
    }

    // Ensure database is created/migrated
    public static async Task<WebApplication> MigrateDatabaseAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            await db.Database.MigrateAsync();

            if (db is AppDbContext appDb)
            {
                // Only dynamic/env data here. Roles/Positions via HasData+migrations.
                await DbSeeder.SeedAsync(appDb);
            }
        }
        catch (Exception ex)
        {
            // surface startup issues early; you can swap for ILogger if you prefer
            var logger = app.Logger;
            logger.LogError(ex, "Database migration/seed failed");
            throw; // fail fast on schema/seed errors
        }

        return app;
    }
}