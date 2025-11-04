using App.Application.Abstractions.Messaging;
using App.Domain.Common;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class OutboxRepository(AppDbContext db) : IOutboxWriter
{
    public void Add(object domainEvent)
    {
        var message = new OutboxMessage(domainEvent);
        db.OutboxMessages.Add(message);
    }
}