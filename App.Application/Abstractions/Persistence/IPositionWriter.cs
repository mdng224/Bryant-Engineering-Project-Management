using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence;

public interface IPositionWriter
{
    Task AddAsync(Position position, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(Guid positionId, CancellationToken ct = default);
    Task<Position?> GetForUpdateAsync(Guid positionId, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}