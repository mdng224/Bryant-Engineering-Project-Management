using App.Application.Common.Dtos;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IClientReader
{
    /*
    Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken ct = default);
*/
    Task<(IReadOnlyList<ClientListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter,
        bool hasActiveProject,
        CancellationToken ct = default);
}