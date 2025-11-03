namespace App.Domain.Common.Abstractions;

/// <summary>
/// Marks an entity as supporting soft deletion.
/// Values are maintained by the domain or persistence layer.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>UTC time when the entity was soft-deleted, or null if active.</summary>
    DateTimeOffset? DeletedAtUtc { get; }

    /// <summary>ID of the user who performed the deletion, or null if system-initiated.</summary>
    Guid? DeletedById { get; }

    /// <summary>Indicates whether the entity has been soft-deleted.</summary>
    bool IsDeleted => DeletedAtUtc.HasValue;
}