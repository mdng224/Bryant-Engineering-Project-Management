using App.Application.Abstractions.Persistence.Readers;
using App.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class ClientTypeReader(AppDbContext db) : IClientTypeReader
{
    public async Task<IReadOnlyList<ClientType>> GetAllAsync(CancellationToken ct = default) =>
        await db.ReadSet<ClientType>()
            .OrderBy(t => t.Name)
            .ToListAsync(ct);
}