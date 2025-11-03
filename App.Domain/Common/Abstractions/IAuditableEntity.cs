namespace App.Domain.Common.Abstractions;

/// <summary>Common audit metadata for entities. Values are set by the persistence layer.</summary>
public interface IAuditableEntity
{
    DateTimeOffset CreatedAtUtc { get; }
    Guid? CreatedById { get; }

    DateTimeOffset UpdatedAtUtc { get; }
    Guid? UpdatedById { get; }

    DateTimeOffset? DeletedAtUtc { get; }
}