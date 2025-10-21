using System.Text.Json;

namespace App.Domain.Common;

public class OutboxMessage
{
    // --- Core Fields --------------------------------------------------------
    public Guid Id { get; private set; }
    public string Type { get; private set; } = null!;
    public string Payload { get; private set; } = null!;
    public int RetryCount { get; private set; }
    public DateTimeOffset OccurredAtUtc { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? ProcessedAtUtc { get; private set; }

    // --- Constructors -------------------------------------------------------
    private OutboxMessage() { }

    public OutboxMessage(object domainEvent)
    {
        Id = Guid.CreateVersion7();
        Type = domainEvent.GetType().FullName!;
        Payload = JsonSerializer.Serialize(domainEvent);
    }

    // --- Mutators -----------------------------------------------------------
    public void MarkProcessed() => ProcessedAtUtc = DateTimeOffset.UtcNow;
    public void IncrementRetry() => RetryCount++;
}