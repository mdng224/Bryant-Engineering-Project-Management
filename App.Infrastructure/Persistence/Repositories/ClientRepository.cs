using App.Application.Abstractions.Persistence.Readers;
using App.Domain.Clients;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class ClientRepository(AppDbContext db) : IClientReader
{
    public async Task<IReadOnlyList<Client>> GetByNameIncludingDeletedAsync(
        string normalizedName,
        CancellationToken ct = default)
    {
        var clients = await db.Clients
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(p => p.Name == normalizedName)
            .ToListAsync(ct);

        return clients;
    }

    public async Task<(IReadOnlyList<Client> clients, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default)
    {
        var query = db.Clients
            .IgnoreQueryFilters()
            .AsNoTracking();
        
        
        switch (isDeleted)
        {
            case true:
                query = query.Where(p => p.DeletedAtUtc != null);
                break;
            case false:
                query = query.Where(p => p.DeletedAtUtc == null);
                break;
            case null:
                break;
        }
        
        // Filter by first/middle/last/company (ILIKE %term%)
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            query = query.Where(c =>
                EF.Functions.ILike(c.ContactFirst!, pattern) ||
                EF.Functions.ILike(c.ContactMiddle!, pattern) ||
                EF.Functions.ILike(c.ContactLast!, pattern) ||
                EF.Functions.ILike(c.Name!, pattern)
            );
        }
        
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);

        // Sort: companies first (company_name not null), then by company_name; 
        // for person entries (company_name null) sort by last/first/middle; finally by Id for stability
        var clients = await query
            .OrderBy(c => c.Name == null)
            .ThenBy(c => c.ContactLast)
            .ThenBy(c => c.ContactFirst)
            .ThenBy(c => c.ContactMiddle)
            .ThenBy(c => c.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
        
        return (clients, totalCount);
    }
}