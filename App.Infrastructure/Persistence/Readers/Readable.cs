using App.Domain.Common.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

internal static class Readable
{
    public static IQueryable<T> ReadSet<T>(this DbContext db)
        where T : class
    {
        return db.Set<T>().AsNoTracking();
    }

    public static IQueryable<T> ApplyDeletedFilter<T>(this IQueryable<T> query, bool? isDeleted)
        where T : class, ISoftDeletable
    {
        // Default (null) means "active" — i.e. DeletedAtUtc == null
        return isDeleted switch
        {
            true          => query.IgnoreQueryFilters().Where(x => x.DeletedAtUtc != null),  // Show deleted
            false or null => query.Where(x => x.DeletedAtUtc == null) // Show active (default)
        };
    }
    
    // Use sparingly; pass ignore:true only on admin/all-rows screens
    public static IQueryable<T> MaybeIgnoreFilters<T>(this IQueryable<T> query, bool ignore)
        where T : class
    {
        return ignore ? query.IgnoreQueryFilters() : query;
    }
}