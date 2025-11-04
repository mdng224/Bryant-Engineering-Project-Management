using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IPositionReader
{
    Task<IReadOnlyList<Position>> GetByNameIncludingDeletedAsync(
        string name,
        CancellationToken ct = default);

    Task<Position?> GetForUpdateAsync(Guid id, CancellationToken ct);
    Task<(IReadOnlyList<Position> positions, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        CancellationToken ct = default);
}