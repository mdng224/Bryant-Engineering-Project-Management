using App.Application.Common.Dtos.Projects;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IProjectReader
{
    /*
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> GetByNameIncludingDeletedAsync(
        string name,
        CancellationToken ct = default);
    Task<Dictionary<Guid,(int Total,int Active)>> GetCountsPerClientAsync(
        IReadOnlyCollection<Guid> clientIds, CancellationToken ct = default);*/
    Task<(IReadOnlyList<ProjectListItemDto> items, int totalCount)> GetPagedAsync(int skip,
        int take,
        string? normalizedNameFilter,
        bool? isDeleted,
        Guid? clientId,
        string? manager,
        CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetDistinctProjectManagersAsync(CancellationToken ct = default);
}