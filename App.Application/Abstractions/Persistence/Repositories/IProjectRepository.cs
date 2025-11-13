using App.Domain.Projects;

namespace App.Application.Abstractions.Persistence.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetAsync(Guid id, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
}