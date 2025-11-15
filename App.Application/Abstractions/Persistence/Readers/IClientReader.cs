using App.Application.Common.Dtos;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IClientReader
{
    Task<bool> EmailExistsAsync(string name, CancellationToken ct = default);
    Task<(IReadOnlyList<ClientListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter,
        bool hasActiveProject,
        CancellationToken ct = default);
}