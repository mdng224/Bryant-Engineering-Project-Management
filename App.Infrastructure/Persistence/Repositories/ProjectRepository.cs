using App.Application.Abstractions.Persistence.Repositories;
using App.Domain.Common.Abstractions;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class ProjectRepository(AppDbContext db) : IProjectRepository
{
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