using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // 1. Roles & Position now handled by HasData/migrations.

        // 2. Employees — link to positions, departments, etc.
        if (!await db.Employees.AnyAsync(ct))
        {
            db.Employees.AddRange(EmployeeSeedFactory.All);
            await db.SaveChangesAsync(ct);
        }

        // Add other env-specific seeds (e.g., default admin user) here if needed.
    }
}