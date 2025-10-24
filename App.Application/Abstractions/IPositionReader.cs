using App.Domain.Employees;

namespace App.Application.Abstractions;

public interface IPositionReader
{
    Task<IReadOnlyList<Position>> GetAllAsync(CancellationToken ct = default);
}