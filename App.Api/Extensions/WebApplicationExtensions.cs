using App.Api.Features.Auth;
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

        return app;
    }

    // Ensure database is created/migrated
    public static WebApplication MigrateDatabase<TContext>(this WebApplication app) where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TContext>();
        db.Database.Migrate();
        return app;
    }
}