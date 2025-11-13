using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class ProjectReader(AppDbContext db) : IProjectReader
{
    // TODO: Move this to repo maybe
    /*
    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var project = await db.Projects
            .AsTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        
        return project;
    }

    public async Task<IReadOnlyList<Project>> GetByNameIncludingDeletedAsync(
        string normalizedName,
        CancellationToken ct = default)
    {
        var query = Query();
        var project = await query
            .Where(p => p.Name == normalizedName)
            .ToListAsync(ct);

        return project;
    }

    public async Task<Dictionary<Guid, (int Total, int Active)>> GetCountsPerClientAsync(
        IReadOnlyCollection<Guid> clientIds,
        CancellationToken ct = default)
    {
        if (clientIds.Count == 0)
            return new Dictionary<Guid, (int Total, int Active)>();

        var query = Query();
        var result = clientIds.ToDictionary(id => id, _ => (Total: 0, Active: 0));
        
        var rows = await query
            .Where(p => clientIds.Contains(p.ClientId))
            .GroupBy(p => p.ClientId)
            .Select(g => new
            {
                ClientId = g.Key,
                Total = g.Count(),
                Active = g.Count(p => p.DeletedAtUtc == null)
            })
            .ToListAsync(ct);
        
        foreach (var row in rows)
            result[row.ClientId] = (row.Total, row.Active);

        return result;
    }*/

    public async Task<(IReadOnlyList<ProjectListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default)
    {
        var query = db.ReadSet<Project>().ApplyDeletedFilter(isDeleted);
        
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

        query = query
            .OrderBy(plid => plid.Year)
            .ThenByDescending(plid => plid.Number)
            .ThenBy(plid => plid.Id);
        
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var items = await query
            .Skip(skip)
            .Take(take)
            .Select(p => new ProjectListItemDto(
                p.Id,
                p.ClientId,
                p.Client.Name,
                p.ScopeId,
                p.Scope.Name,
                p.Name,
                p.Code,
                p.Year,
                p.Number,
                p.Manager,
                p.Type,
                p.Location,
                p.CreatedAtUtc,
                p.UpdatedAtUtc,
                p.DeletedAtUtc,
                p.CreatedById,
                p.UpdatedById,
                p.DeletedById
            ))
            .ToListAsync(ct);
        
        return (items, totalCount);
    }
}