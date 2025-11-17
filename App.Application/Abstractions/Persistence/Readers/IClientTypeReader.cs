using App.Domain.Clients;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IClientTypeReader
{
    Task<IReadOnlyList<ClientType>> GetAllAsync(CancellationToken ct = default);
}