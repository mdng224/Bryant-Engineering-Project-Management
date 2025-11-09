using App.Domain.Clients;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IClientReader
{
    Task<IReadOnlyList<Client>> GetByNameIncludingDeletedAsync(
        string name,
        CancellationToken ct = default);
    Task<(IReadOnlyList<Client> clients, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default);
}