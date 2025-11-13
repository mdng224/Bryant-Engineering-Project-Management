using App.Domain.Clients;

namespace App.Application.Abstractions.Persistence.Repositories;

public interface IClientRepository
{
    void Add(Client client);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default);
    void Update(Client client);
}