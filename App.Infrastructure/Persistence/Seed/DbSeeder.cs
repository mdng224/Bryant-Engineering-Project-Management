using App.Domain.Clients;
using App.Domain.Projects;
using App.Infrastructure.Persistence.Seed.Configurations;
using App.Infrastructure.Persistence.Seed.Factories;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    private static readonly string BaseDir = AppContext.BaseDirectory;
    private static readonly string ClientsPath  = Path.Combine(BaseDir, "Persistence", "Seed", "Data", "Clients.csv");
    private static readonly string ProjectsPath = Path.Combine(BaseDir, "Persistence", "Seed", "Data", "Projects.csv");
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        await SeedIfEmptyAsync(db, RoleSeedFactory.All, ct);
        await SeedIfEmptyAsync(db, PositionSeedFactory.All, ct);
        await SeedIfEmptyAsync(db, EmployeeSeedFactory.All, ct);
        await SeedIfEmptyAsync(db, ClientCategorySeedFactory.All, ct);
        await SeedIfEmptyAsync(db, ClientTypeSeedFactory.All, ct);
        await SeedClientIfNotEmptyAsync(db, ct);
        await SeedIfEmptyAsync(db, ScopeSeedFactory.All, ct);
        await db.SaveChangesAsync(ct);  // ✅ Save the above before we seed projects (which need Clients + Scopes)
        await SeedProjectsIfEmptyAsync(db, ct);

        await db.SaveChangesAsync(ct); // AuditSaveChangesInterceptor will stamp
    }
    
    private static async Task SeedIfEmptyAsync<TEntity>(
        AppDbContext db,
        IEnumerable<TEntity> seedData,
        CancellationToken ct)
        where TEntity : class
    {
        var set = db.Set<TEntity>();

        if (await set.AnyAsync(ct))
        {
            Console.WriteLine($"Skipping {typeof(TEntity).Name} seeding — already has data.");
            return;
        }

        var list = seedData as IList<TEntity> ?? seedData.ToList(); // ✅ materialize once
        set.AddRange(list);
        Console.WriteLine($"Seeded {typeof(TEntity).Name} records.");
    }

    private static async Task SeedClientIfNotEmptyAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Clients.AnyAsync(ct))
        {
            Console.WriteLine("Skipping client seeding — already has data.");
            return;
        }

        if (!File.Exists(ClientsPath))
        {
            Console.WriteLine($"Client seed skipped — file not found: {ClientsPath}");
            return;
        }

        // Preload lookup dictionaries (name -> id), normalized
        var clientCategoryNameIdMap = await db.ClientCategories
            .AsNoTracking()
            .ToDictionaryAsync(cc => Normalize(cc.Name), cc => cc.Id, ct);

        var typeByName = await db.ClientTypes
            .AsNoTracking()
            .ToDictionaryAsync(c => Normalize(c.Name), c => c.Id, ct);
        
        using var reader = File.OpenText(ClientsPath);
        var clients = ClientSeedFactory.Enumerate(reader)
            .Select(cs =>
            {
                // Resolve category/type IDs by name (case/space tolerant)
                var categoryId = TryLookupId(clientCategoryNameIdMap, cs.ClientCategory)
                                 ?? ClientCategoryIds.OtherMiscellaneous;
                var typeId = TryLookupId(typeByName, cs.ClientType)
                             ?? ClientTypeIds.UnknownToBeClassified;

                return Client.Seed(
                    clientName:       cs.ClientName ?? "Unknown",
                    namePrefix:       cs.NamePrefix,
                    firstName:        cs.FirstName ?? "Unknown",
                    lastName:         cs.LastName ?? "Unknown",
                    nameSuffix:       cs.NameSuffix,
                    email:            cs.Email,
                    phone:            cs.Phone,
                    line1:            cs.Line1,
                    line2:            cs.Line2,
                    city:             cs.City,
                    state:            cs.State,
                    postalCode:       cs.PostalCode,
                    note:             cs.Note,
                    clientCategoryId: categoryId,
                    clientTypeId:     typeId,
                    legacyProjectCode: null // CSV doesn't provide; Will set later
                );
            })
            .ToList();

        db.Clients.AddRange(clients);
        Console.WriteLine($"Seeded {clients.Count} Client records from {ClientsPath}.");
    }
    
    private static async Task SeedProjectsIfEmptyAsync(AppDbContext db, CancellationToken ct)
    {
        if (await db.Projects.AnyAsync(ct))
        {
            Console.WriteLine("Skipping project seeding — already has data.");
            return;
        }

        if (!File.Exists(ProjectsPath))
        {
            Console.WriteLine($"Project seed skipped — file not found: {ProjectsPath}");
            return;
        }

        // --- Build lookups up front (materialize, then normalize in-memory) ----------
        var clientRows = await db.Clients
            .AsNoTracking()
            .Select(c => new { c.Id, c.Name })
            .ToListAsync(ct);

        var clientsByName = clientRows
            .GroupBy(x => Normalize(x.Name))
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Id).OrderBy(id => id).ToList()
            );

        var scopeRows = await db.Scopes
            .AsNoTracking()
            .Select(s => new { s.Id, s.Name })
            .ToListAsync(ct);

        var scopesByName = scopeRows
            .GroupBy(x => Normalize(x.Name))
            .ToDictionary(
                g => g.Key,
                g => g.First().Id
            );

        using var reader = File.OpenText(ProjectsPath);
        var seeds = ProjectSeedFactory.Enumerate(reader).ToList();

        var projects = new List<Project>(seeds.Count);
        foreach (var r in seeds)
        {
            var clientId = ResolveClientId(r.ClientNameFinal, clientsByName);
            var scopeId = ResolveScopeId(r.Scope, scopesByName, ScopeIds.Unknown);
            // Create project
            var p = Project.Seed(
                clientId:    clientId,
                scopeId:     scopeId,
                name:        r.ProjectName,
                projectCode: r.ProjectCode,
                manager:     r.PM,
                status:      r.Status,
                location:    r.Location,
                type:        r.Type,
                deletedNow:  DateTimeOffset.UtcNow
            );

            projects.Add(p);
        }

        db.Projects.AddRange(projects);
        Console.WriteLine($"Seeded {projects.Count} Project records from {ProjectsPath}.");

        // ---- Backfill client.ProjectCode (first known project for that client) ----
        // Load just the client ids that appeared
        var affectedClientIds = projects.Select(p => p.ClientId).Distinct().ToList();
        var affectedClients = await db.Clients
            .Where(c => affectedClientIds.Contains(c.Id))
            .ToListAsync(ct);

        // choose a deterministic code per client (e.g., earliest by {Year,Number})
        var byClient = projects
            .GroupBy(p => p.ClientId)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(p => p.Year).ThenBy(p => p.Number).First().Code
            );

        var backfilled = 0;
        foreach (var client in affectedClients.Where(c => string.IsNullOrWhiteSpace(c.ProjectCode)))
        {
            if (!byClient.TryGetValue(client.Id, out var code)) continue;

            client.SetLegacyProjectCode(code);
            backfilled++;
        }

        Console.WriteLine($"Backfilled ProjectCode on {backfilled} Client records.");
    }
    
    // ----------------- helpers -----------------

    private static string Normalize(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? string.Empty
            : System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ").ToLowerInvariant();

    private static Guid ResolveClientId(
        string? clientName,
        Dictionary<string, List<Guid>> clientsByName)
    {
        var key = Normalize(clientName);

        if (!clientsByName.TryGetValue(key, out var candidateIds) || candidateIds.Count == 0)
            throw new InvalidOperationException($"Unknown client '{clientName}'.");

        if (candidateIds.Count > 1)
            Console.WriteLine($"⚠️ Duplicate client name '{clientName}' has {candidateIds.Count} records. Using first id.");

        return candidateIds[0]; // deterministic pick
    }
    
    private static Guid ResolveScopeId(
        string? rawScope,
        Dictionary<string, Guid> scopesByName,
        Guid unknownScopeId)
    {
        var key = Normalize(rawScope);

        if (string.IsNullOrWhiteSpace(key) ||
            key is "unknown" or "n/a" or "na" or "tbd")
            return unknownScopeId;

        if (scopesByName.TryGetValue(key, out var id))
            return id;
        
        Console.WriteLine($"⚠️ Unknown scope '{rawScope}' — defaulting to UNKNOWN.");
        return unknownScopeId;
    }
    
    private static Guid? TryLookupId(Dictionary<string, Guid> map, string? key)
    {
        var k = Normalize(key);
        if (k.Length == 0) return null;
        return map.TryGetValue(k, out var id) ? id : null;
    }
}