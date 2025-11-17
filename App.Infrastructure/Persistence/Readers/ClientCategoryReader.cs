using App.Application.Abstractions.Persistence.Readers;
using App.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class ClientCategoryReader(AppDbContext  db) : IClientCategoryReader
{
    public async Task<IReadOnlyList<ClientCategory>> GetAllAsync(CancellationToken ct = default) =>
        await db.ReadSet<ClientCategory>()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
}