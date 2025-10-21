namespace App.Application.Abstractions;

public interface IOutboxWriter
{
    Task AddAsync(object domainEvent, CancellationToken ct = default);
}