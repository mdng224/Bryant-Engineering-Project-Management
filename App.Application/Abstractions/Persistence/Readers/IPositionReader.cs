using App.Application.Common.Dtos;
using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IPositionReader
{
    Task<Position?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Position>> GetByNameIncludingDeletedAsync(
        string name,
        CancellationToken ct = default);

    Task<Position?> GetForUpdateAsync(Guid id, CancellationToken ct);
    Task<(IReadOnlyList<PositionListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default);
}