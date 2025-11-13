using App.Application.Abstractions.Persistence.Readers;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public class ScopeReader(AppDbContext db) : IScopeReader
{
    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, string>();
        
        var scopeNames = await db.ReadSet<Scope>()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(map => map.Id,
                map => map.Name,
                ct);

        return scopeNames;
    }
}