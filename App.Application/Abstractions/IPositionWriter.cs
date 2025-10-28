using App.Domain.Employees;

namespace App.Application.Abstractions;

public interface IPositionWriter
{
    Task AddAsync(Position position, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}