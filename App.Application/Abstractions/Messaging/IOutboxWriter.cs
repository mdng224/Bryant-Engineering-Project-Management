namespace App.Application.Abstractions.Messaging;

public interface IOutboxWriter
{
    Task AddAsync(object domainEvent, CancellationToken ct = default);
}