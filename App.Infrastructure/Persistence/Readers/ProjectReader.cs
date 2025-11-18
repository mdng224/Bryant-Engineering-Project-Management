using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Dtos.Projects;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class ProjectReader(AppDbContext db) : IProjectReader
{
    public async Task<IReadOnlyList<string>> GetDistinctProjectManagersAsync(CancellationToken ct = default)
    {
        var projectManagers = await db.ReadSet<Project>()
            .IgnoreQueryFilters()
            .Select(p => p.Manager)
            .Distinct()
            .OrderBy(m => m)
            .ToListAsync(ct);

        return projectManagers;
    }
    
    public async Task<(IReadOnlyList<ProjectListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter,
        bool? isDeleted,
        Guid? clientId,
        string? manager,
        CancellationToken ct = default)
    {
        var projectQuery = BuildProjectQuery(normalizedNameFilter, isDeleted, clientId, manager);
        
        var totalCount = await projectQuery.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var items = await projectQuery
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

    private IQueryable<Project> BuildProjectQuery(
        string? normalizedNameFilter,
        bool? isDeleted,
        Guid? clientId,
        string? manager)
    {
        var projectQuery = db.ReadSet<Project>().ApplyDeletedFilter(isDeleted);

        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            projectQuery = projectQuery.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

        if (clientId is not null)
            projectQuery = projectQuery.Where(p => p.ClientId == clientId);
        
        if  (manager is not null)
            projectQuery = projectQuery.Where(p => p.Manager == manager);

        return projectQuery
            .OrderBy(p => p.Year)
            .ThenByDescending(p => p.Number)
            .ThenBy(p => p.Id);
    }
}