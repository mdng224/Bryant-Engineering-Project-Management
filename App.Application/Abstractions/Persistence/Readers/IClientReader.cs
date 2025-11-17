using App.Application.Common.Dtos;
using App.Application.Common.Dtos.Clients;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IClientReader
{
    Task<bool> EmailExistsAsync(string name, CancellationToken ct = default);
    Task<(IReadOnlyList<ClientListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter,
        bool hasActiveProject,
        Guid? categoryId,
        Guid? typeId,
        CancellationToken ct = default);
}