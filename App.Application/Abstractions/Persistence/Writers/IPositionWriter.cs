using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Writers;

public interface IPositionWriter
{
    void Add(Position position);
    Task<bool> SoftDeleteAsync(Guid positionId, CancellationToken ct = default);
    void Update(Position position);
}