using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Repositories;

public interface IPositionRepository
{
    void Add(Position position);
    Task<Position?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Position?> GetForUpdateAsync(Guid id, CancellationToken ct);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
    void Update(Position position);
}