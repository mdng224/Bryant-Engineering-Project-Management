using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Domain.Clients;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class ClientReader(AppDbContext db) : IClientReader
{
    /*
    public async Task<IReadOnlyDictionary<Guid, string>> GetNamesByIdsAsync(
        IReadOnlyList<Guid> ids,
        CancellationToken ct = default)
    {
        if (ids.Count == 0)
            return new Dictionary<Guid, string>();
        var query = Query();

        var clientIdNameMap = await query
            .Where(c => ids.Contains(c.Id))
            .Select(c => new
            {
                c.Id,
                c.Name
            })
            .ToDictionaryAsync(
                map => mac.Id,
                map => mac.Name,
                ct);

        return clientIdNameMap;
    }*/
    
    public async Task<(IReadOnlyList<ClientListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default)
    {
        // 2) Create base client query
        var clients = db.ReadSet<Client>().ApplyDeletedFilter(isDeleted);
        
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            clients = clients.Where(c =>
                EF.Functions.ILike(c.FirstName ?? "", pattern) ||
                EF.Functions.ILike(c.LastName  ?? "", pattern) ||
                EF.Functions.ILike(c.Name      ?? "", pattern));
        }
        
        var query = clients.Select(c => new
        {
            Client = c,
            TotalProjects = db.Projects
                .IgnoreQueryFilters()
                .Count(p => p.ClientId == c.Id),
            TotalActiveProjects = db.Projects
                .Count(p => p.ClientId == c.Id) // global filter keeps only active
        });

        
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);

        var orderedQuery = query
            .OrderByDescending(jq => jq.TotalActiveProjects)
            .ThenByDescending(jq => jq.TotalProjects)
            .ThenBy(jq => jq.Client.Name)
            .ThenBy(jq => jq.Client.Id);
            
        var items = await orderedQuery
            .Skip(skip)
            .Take(take)
            .Select(clid => new ClientListItemDto(
                clid.Client.Id,
                clid.Client.Name,                 // required
                clid.TotalActiveProjects,
                clid.TotalProjects,
                clid.Client.FirstName,
                clid.Client.LastName,
                clid.Client.Email,
                clid.Client.Phone,
                clid.Client.Address,              // owned type is fine in projection
                clid.Client.Note,
                clid.Client.CreatedAtUtc,
                clid.Client.UpdatedAtUtc,
                clid.Client.DeletedAtUtc,
                clid.Client.CreatedById,
                clid.Client.UpdatedById,
                clid.Client.DeletedById))
            .ToListAsync(ct);
        
        return (items, totalCount);
    }
}