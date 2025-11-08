using System.Text;
using App.Domain.Clients;
using App.Domain.Projects;
using App.Infrastructure.Persistence.Seed.Common;
using App.Infrastructure.Persistence.Seed.Factories;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // 1) Roles (non-auditable)
        {
            var existing = await db.Roles.AsNoTracking().Select(x => x.Id).ToListAsync(ct);
            var existingSet = existing.Count == 0 ? null : new HashSet<Guid>(existing);
            var toAdd = existingSet is null
                ? RoleSeedFactory.All.ToList()
                : RoleSeedFactory.All.Where(s => !existingSet.Contains(s.Id)).ToList();

            if (toAdd.Count > 0) db.Roles.AddRange(toAdd);
        }

        // 2) Positions (auditable) — interceptor will stamp audit fields
        {
            var existing = await db.Positions.AsNoTracking().Select(x => x.Id).ToListAsync(ct);
            var existingSet = existing.Count == 0 ? null : new HashSet<Guid>(existing);
            var toAdd = existingSet is null
                ? PositionSeedFactory.All.ToList()
                : PositionSeedFactory.All.Where(s => !existingSet.Contains(s.Id)).ToList();

            if (toAdd.Count > 0) db.Positions.AddRange(toAdd);
        }

        // 3) Employees (auditable) — interceptor will stamp audit fields
        {
            var existing = await db.Employees.AsNoTracking().Select(x => x.Id).ToListAsync(ct);
            var existingSet = existing.Count == 0 ? null : new HashSet<Guid>(existing);
            var toAdd = existingSet is null
                ? EmployeeSeedFactory.All.ToList()
                : EmployeeSeedFactory.All.Where(s => !existingSet.Contains(s.Id)).ToList();

            if (toAdd.Count > 0) db.Employees.AddRange(toAdd);
        }
        
        await SeedClientsAndProjectsSinglePass(db, ct);
        

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync(ct); // AuditSaveChangesInterceptor will stamp
    }
    
    // Single-pass client+project seeding from the combined CSV
    private static async Task SeedClientsAndProjectsSinglePass(AppDbContext db, CancellationToken ct)
    {
        // Seed only if BOTH tables are empty
        if (await db.Clients.AsNoTracking().AnyAsync(ct) || await db.Projects.AsNoTracking().AnyAsync(ct))
        {
            Console.WriteLine("[Seed] Skipped single-pass (clients or projects not empty).");
            return;
        }

        // In-memory trackers for this run
        var createdClientsByName = new Dictionary<string, Client>(StringComparer.OrdinalIgnoreCase);
        var createdProjectCodes  = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var reader = SeedResources.OpenText(
            "App.Infrastructure.Persistence.Seed.Data.ClientsProjects.csv",
            Encoding.UTF8);

        foreach (var row in ClientProjectSeedFactory.Enumerate(reader))
        {
            // Require a project code
            if (string.IsNullOrWhiteSpace(row.ProjectCode))
            {
                Console.WriteLine("[Seed] Skipping row with empty PROJECT Code.");
                continue;
            }
            if (createdProjectCodes.Contains(row.ProjectCode))
                continue;

            // Require a client name
            var clientName = row.ClientName;
            if (string.IsNullOrWhiteSpace(clientName))
            {
                Console.WriteLine($"[Seed] No client name for project code '{row.ProjectCode}'. Skipping row.");
                continue;
            }

            // Reuse or create the client in-memory
            if (!createdClientsByName.TryGetValue(clientName, out var client))
            {
                var (first, last) = ClientProjectSeedFactory.TrySplitLastFirst(row.ProjectContact);
                // If your Client.Seed signature accepts projectCode and stores it, keep it; otherwise remove it.
                client = Client.Seed(
                    clientName:   clientName.ToLowerInvariant(),
                    contactFirst: first?.ToLowerInvariant(),
                    contactLast:  last?.ToLowerInvariant(),
                    projectCode:  row.ProjectCode
                );
                db.Clients.Add(client);
                createdClientsByName[clientName] = client;
            }

            // Create the project
            var project = Project.Seed(
                clientId:    client.Id,
                name:        row.ProjectName.ToLowerInvariant(),
                projectCode: row.ProjectCode,
                scope:       row.Scope.ToLowerInvariant(),
                manager:     row.PM.ToLowerInvariant(),
                status:      row.Status.ToLowerInvariant(),
                location:    row.Location.ToLowerInvariant(),
                type:        row.Type.ToLowerInvariant()
            );

            db.Projects.Add(project);
            createdProjectCodes.Add(row.ProjectCode);
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync(ct);
        else
            Console.WriteLine("[Seed] Nothing to insert.");
    }
}