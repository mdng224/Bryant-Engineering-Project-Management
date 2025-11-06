using App.Domain.Clients;
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
            var toAdd = (existingSet is null)
                ? RoleSeedFactory.All.ToList()
                : RoleSeedFactory.All.Where(s => !existingSet.Contains(s.Id)).ToList();

            if (toAdd.Count > 0) db.Roles.AddRange(toAdd);
        }

        // 2) Positions (auditable) — interceptor will stamp audit fields
        {
            var existing = await db.Positions.AsNoTracking().Select(x => x.Id).ToListAsync(ct);
            var existingSet = existing.Count == 0 ? null : new HashSet<Guid>(existing);
            var toAdd = (existingSet is null)
                ? PositionSeedFactory.All.ToList()
                : PositionSeedFactory.All.Where(s => !existingSet.Contains(s.Id)).ToList();

            if (toAdd.Count > 0) db.Positions.AddRange(toAdd);
        }

        // 3) Employees (auditable) — interceptor will stamp audit fields
        {
            var existing = await db.Employees.AsNoTracking().Select(x => x.Id).ToListAsync(ct);
            var existingSet = existing.Count == 0 ? null : new HashSet<Guid>(existing);
            var toAdd = (existingSet is null)
                ? EmployeeSeedFactory.All.ToList()
                : EmployeeSeedFactory.All.Where(s => !existingSet.Contains(s.Id)).ToList();

            if (toAdd.Count > 0) db.Employees.AddRange(toAdd);
        }
        
        // 4) Clients (auditable) — interceptor will stamp audit fields
        {
            var clientSeeds = ClientSeedFactory.LoadUniqueByClientName(out var warnings);

            foreach (var warning in warnings)
                Console.WriteLine($"[ClientSeed]:\t{warning}");

            // Get existing client names to avoid duplicates on re-run
            var existingNames = await db.Clients
                .AsNoTracking()
                .Select(x => x.Name)
                .ToListAsync(ct);

            var existingNameSet = new HashSet<string>(existingNames, StringComparer.OrdinalIgnoreCase);

            var toAdd = new List<Client>();

            foreach (var seed in clientSeeds.Where(seed => !existingNameSet.Contains(seed.ClientName)))
            {
                var (contactFirst, contactLast) = ClientSeedFactory.TrySplitLastFirst(seed.ContactRaw);
                
                var client = Client.Seed(seed.ClientName, contactFirst,  contactLast);
                toAdd.Add(client);
            }

            if (toAdd.Count > 0) db.Clients.AddRange(toAdd);
        }

        if (db.ChangeTracker.HasChanges())
            await db.SaveChangesAsync(ct); // AuditSaveChangesInterceptor will stamp
    }
}