using App.Application.Common.Dtos.Clients;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IClientReader
{
    Task<ClientDetailsDto?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<ClientRowDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter,
        bool hasActiveProject,
        Guid? categoryId,
        Guid? typeId,
        CancellationToken ct = default);
}