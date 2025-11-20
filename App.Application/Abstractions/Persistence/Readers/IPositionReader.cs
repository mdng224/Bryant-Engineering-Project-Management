using App.Application.Common.Dtos;
using App.Application.Common.Dtos.Positions;
using App.Application.Common.Dtos.Projects;
using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IPositionReader
{
    Task<IReadOnlyList<Position>> GetByNameIncludingDeletedAsync(
        string name,
        CancellationToken ct = default);

    Task<(IReadOnlyList<PositionListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default);
}