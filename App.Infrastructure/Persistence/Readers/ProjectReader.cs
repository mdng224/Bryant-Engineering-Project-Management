using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class ProjectReader(AppDbContext db) : IProjectReader
{
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