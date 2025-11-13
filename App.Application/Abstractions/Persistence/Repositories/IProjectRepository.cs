namespace App.Application.Abstractions.Persistence.Repositories;

public interface IProjectRepository
{
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
}