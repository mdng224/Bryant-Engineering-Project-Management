using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Repositories;

public interface IPositionRepository
{
    void Add(Position position);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
    void Update(Position position);
}