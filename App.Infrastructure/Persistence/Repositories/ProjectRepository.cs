using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Writers;
using App.Domain.Common.Abstractions;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(AppDbContext db) : IProjectReader, IProjectWriter
{
    // -------------------- Readers --------------------
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
        var project = await db.Projects
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(p => p.Name == normalizedName)
            .ToListAsync(ct);

        return project;
    }

    public async Task<(IReadOnlyList<Project> projects, int totalCount)> GetPagedAsync(int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default)
    {
        var query = db.Projects
            .IgnoreQueryFilters()
            .AsNoTracking();
        
        switch (isDeleted)
        {
            case true:
                query = query.Where(p => p.DeletedAtUtc != null);
                break;
            case false:
                query = query.Where(p => p.DeletedAtUtc == null);
                break;
            case null:
                break;
        }
        
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }
        
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var projects = await query
            .Include(p => p.Client)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .AsSplitQuery()
            .ToListAsync(ct);
        
        return (projects, totalCount);
    }

    // -------------------- Writers --------------------
    public void Add(Project project) => db.Projects.Add(project);

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var project = await db.Projects.FindAsync([id], ct);

        switch (project)
        {
            case null:
                return false;
            case ISoftDeletable { DeletedAtUtc: not null }:
                return true;
            default:
                db.Projects.Remove(project); // interceptor flips to soft-delete
                return true;
        }
    }

    public void Update(Project project) => db.Projects.Update(project);
}