using App.Domain.Clients;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IClientCategoryReader
{
    Task<IReadOnlyList<ClientCategory>> GetAllAsync(CancellationToken ct = default);
}