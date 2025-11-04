namespace App.Application.Abstractions.Messaging;

public interface IOutboxWriter
{
    void Add(object domainEvent);
}