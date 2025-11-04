using App.Domain.Clients;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IClientReader
{
    Task<(IReadOnlyList<Client> clients, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        CancellationToken ct = default);
}