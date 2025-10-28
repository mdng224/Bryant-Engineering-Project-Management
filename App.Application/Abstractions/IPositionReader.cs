using App.Domain.Employees;

namespace App.Application.Abstractions;

public interface IPositionReader
{
    Task<(IReadOnlyList<Position> positions, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        CancellationToken ct = default);
}