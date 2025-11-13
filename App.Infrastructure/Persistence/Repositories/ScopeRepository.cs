using App.Application.Abstractions.Persistence.Readers;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public class ScopeRepository(AppDbContext db) : IScopeReader
{
    // -------------------- Readers --------------------

    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, string>();
        
        var query = Query();
        var scopeNames = await query
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(map => map.Id,
                map => map.Name,
                ct);

        return scopeNames;
    }
    
    private IQueryable<Scope> Query(bool includeDeleted = false)
    {
        var query = db.Scopes.AsNoTracking();
        return includeDeleted ? query.IgnoreQueryFilters() : query;
    }
}