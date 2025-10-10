namespace App.Domain.Common;

/// <summary>
/// Marker contract for entities that participate in auditing.
/// <para>
/// Implementing this interface guarantees that an entity exposes
/// the timestamps needed for audit trails:
/// <see cref="CreatedAtUtc"/>, <see cref="UpdatedAtUtc"/>, and <see cref="DeletedAtUtc"/>.
/// </para>
/// <para>
/// **Why read-only?**
/// These values are owned and maintained by the infrastructure/persistence layer
/// (e.g., EF Core SaveChanges interceptors or database defaults). Exposing them
/// as getters only prevents accidental mutation from application code and keeps
/// the domain model's invariants intact.
/// </para>
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// The UTC timestamp when the entity was first created/persisted.
    /// Read-only to consumers; set by the persistence layer.
    /// </summary>
    DateTimeOffset CreatedAtUtc     { get; }

    /// <summary>
    /// The UTC timestamp when the entity was last modified.
    /// Read-only to consumers; updated by the persistence layer on each save.
    /// </summary>
    DateTimeOffset UpdatedAtUtc     { get; }

    /// <summary>
    /// The UTC timestamp when the entity was soft-deleted, or <c>null</c> if not deleted.
    /// Read-only to consumers; set/cleared by domain methods (e.g., <c>SoftDelete()</c>/<c>Restore()</c>)
    /// and persisted by the infrastructure layer.
    /// </summary>
    DateTimeOffset? DeletedAtUtc    { get; }
}
