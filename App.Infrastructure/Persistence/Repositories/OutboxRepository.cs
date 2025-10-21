using App.Application.Abstractions;
using App.Domain.Common;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository(AppDbContext db) : IOutboxWriter
{
    public async Task AddAsync(object domainEvent, CancellationToken ct = default)
    {
        var message = new OutboxMessage(domainEvent);
        await db.OutboxMessages.AddAsync(message, ct);
    }
}