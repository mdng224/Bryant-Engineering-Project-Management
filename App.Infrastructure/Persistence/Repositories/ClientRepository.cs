using App.Application.Abstractions.Persistence.Repositories;
using App.Domain.Clients;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class ClientRepository(AppDbContext db) : IClientRepository
{
    public void Add(Client client) => db.Clients.Add(client);

    public Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public void Update(Client client)
    {
        throw new NotImplementedException();
    }
}