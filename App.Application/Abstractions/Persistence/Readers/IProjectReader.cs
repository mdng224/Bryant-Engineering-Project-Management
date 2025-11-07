using App.Domain.Projects;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IProjectReader
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Project>> GetByNameIncludingDeletedAsync(
        string name,
        CancellationToken ct = default);
    Task<(IReadOnlyList<Project> projects, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default);
}