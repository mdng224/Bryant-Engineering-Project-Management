using App.Application.Common.Dtos.Projects;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IProjectReader
{
    Task<ProjectDetailsDto?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<ProjectRowDto> items, int totalCount)> GetPagedAsync(int skip,
        int take,
        string? normalizedNameFilter,
        bool? isDeleted,
        Guid? clientId,
        string? manager,
        CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetDistinctProjectManagersAsync(CancellationToken ct = default);
}