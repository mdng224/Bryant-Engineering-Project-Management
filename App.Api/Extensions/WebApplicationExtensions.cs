using App.Api.Features.Auth;
using App.Api.Features.Clients;
using App.Api.Features.Employees;
using App.Api.Features.Positions;
using App.Api.Features.Projects;
using App.Api.Features.Users;
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
        app.MapAuthEndpoints();
        app.MapEmployeeEndpoints();
        app.MapPositionEndpoints();
        app.MapUserEndpoints();
        app.MapVerificationEndpoints();
        app.MapClientEndpoints();
        app.MapProjectEndpoints();
        app.MapClientLookupEndpoints();
        app.MapProjectLookupEndpoints();
        
        return app;
    }

    // Ensure database is created/migrated
    public static async Task MigrateDatabaseAsync<TContext>(this WebApplication app)
        where TContext : DbContext
    {
        await using var scope = app.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            await db.Database.MigrateAsync();

            if (db is AppDbContext appDb)
                await DbSeeder.SeedAsync(appDb);
        }
        catch (Exception ex)
        {
            // surface startup issues early; you can swap for ILogger if you prefer
            var logger = app.Logger;
            logger.LogError(ex, "Database migration/seed failed");
            throw; // fail fast on schema/seed errors
        }
    }
}