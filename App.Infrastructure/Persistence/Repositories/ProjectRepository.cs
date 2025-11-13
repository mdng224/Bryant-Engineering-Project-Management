using App.Application.Abstractions.Persistence.Repositories;
using App.Domain.Common.Abstractions;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(AppDbContext db) : IProjectRepository
{
    public async Task<Project?> GetAsync(Guid id, CancellationToken ct)
    {
        var project = await db.Projects
            .AsTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        
        return project;
    }
    
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
}