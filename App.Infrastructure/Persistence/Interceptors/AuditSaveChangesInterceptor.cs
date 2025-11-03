using App.Application.Abstractions.Security;
using App.Domain.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace App.Infrastructure.Persistence.Interceptors;

public sealed class AuditSaveChangesInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    private const string CreatedAtName = nameof(IAuditableEntity.CreatedAtUtc);
    private const string CreatedByName = nameof(IAuditableEntity.CreatedById);
    private const string UpdatedAtName = nameof(IAuditableEntity.UpdatedAtUtc);
    private const string UpdatedByName = nameof(IAuditableEntity.UpdatedById);
    private const string DeletedAtName = nameof(ISoftDeletable.DeletedAtUtc);
    private const string DeletedByName = nameof(ISoftDeletable.DeletedById);
    
    private static readonly HashSet<string> AuditProps = new(
    [
        CreatedAtName, CreatedByName, UpdatedAtName, UpdatedByName, DeletedAtName, DeletedByName
    ], StringComparer.Ordinal);
    
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Stamp(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        Stamp(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void Stamp(DbContext? dbContext)
    {
        if (dbContext is null) return;

        var now = DateTimeOffset.UtcNow;
        var userId = currentUser.UserId;

        foreach (var entry in dbContext.ChangeTracker.Entries())
        {
            // Soft delete transform
            if (entry.State == EntityState.Deleted && HasSoftDeleteProps(entry))
            {
                entry.State = EntityState.Modified;
                SetIfExists(entry, DeletedAtName, now);
                SetIfExists(entry, DeletedByName, userId);
                SetIfExists(entry, UpdatedAtName, now);
                SetIfExists(entry, UpdatedByName, userId);
                continue;
            }

            if (!HasAuditableProps(entry)) continue;

            switch (entry.State)
            {
                case EntityState.Added:
                    TrySetIfDefault(entry, CreatedAtName, now);
                    TrySetIfDefault(entry, CreatedByName, userId);
                    SetIfExists(entry, UpdatedAtName, now);
                    SetIfExists(entry, UpdatedByName, userId);
                    break;

                case EntityState.Modified:
                    if (HasNonAuditChanges(entry))
                    {
                        SetIfExists(entry, UpdatedAtName, now);
                        SetIfExists(entry, UpdatedByName, userId);
                    }
                    break;
                case EntityState.Detached:
                case EntityState.Unchanged:
                case EntityState.Deleted:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    private static bool HasAuditableProps(EntityEntry e) =>
        e.Metadata.FindProperty(CreatedAtName) is not null &&
        e.Metadata.FindProperty(UpdatedAtName) is not null;

    private static bool HasSoftDeleteProps(EntityEntry e) =>
        e.Metadata.FindProperty(DeletedAtName) is not null;

    private static bool HasNonAuditChanges(EntityEntry entry)
    {
        // If any property is marked modified that isn't one of the audit props
        // we consider it a "real" modification
        return entry.Properties.Any(p => p.IsModified && !AuditProps.Contains(p.Metadata.Name));
    }

    private static void SetIfExists(EntityEntry entry, string propertyName, object? value)
    {
        if (entry.Metadata.FindProperty(propertyName) is null) return;
        entry.Property(propertyName).CurrentValue = value;
    }

    private static void TrySetIfDefault(EntityEntry entry, string propertyName, object? value)
    {
        if (entry.Metadata.FindProperty(propertyName) is null) return;

        var prop = entry.Property(propertyName);

        switch (prop.CurrentValue)
        {
            // Only assign when current value is the type's default
            case null:
                prop.CurrentValue = value;
                return;
            // Handle DateTimeOffset default(…) and Guid default(…)
            case DateTimeOffset dto when dto == default:
            case Guid g when g == Guid.Empty:
                prop.CurrentValue = value;
                break;
        }
    }
}