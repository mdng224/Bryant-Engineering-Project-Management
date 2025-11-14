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
        string? normalizedNameFilter,
        bool hasActiveProject,
        CancellationToken ct = default)
    {
        var clientQuery = db.ReadSet<Client>();
        
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            clientQuery = clientQuery.Where(c =>
                EF.Functions.ILike(c.FirstName ?? "", pattern) ||
                EF.Functions.ILike(c.LastName  ?? "", pattern) ||
                EF.Functions.ILike(c.Name      ?? "", pattern));
        }
        

        var queryWithCounts =
            from c in clientQuery
            join p in db.ReadSet<Project>().IgnoreQueryFilters()
                on c.Id equals p.ClientId into projGroup
            from pg in projGroup.DefaultIfEmpty()
            group pg by c into g
            select new
            {
                Client = g.Key,
                TotalProjects = g.Count(p => p != null),
                TotalActiveProjects = g.Count(p => p != null && p.DeletedAtUtc == null)
            };

        queryWithCounts = hasActiveProject
            ? queryWithCounts.Where(x => x.TotalActiveProjects > 0)
            : queryWithCounts.Where(x => x.TotalActiveProjects == 0);

        var totalCount = await queryWithCounts.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);

        var items = await queryWithCounts
            .OrderByDescending(jq => jq.TotalActiveProjects)
            .ThenByDescending(jq => jq.TotalProjects)
            .ThenBy(jq => jq.Client.Name)
            .ThenBy(jq => jq.Client.Id)
            .Skip(skip)
            .Take(take)
            .Select(x => new ClientListItemDto(
                x.Client.Id,
                x.Client.Name,
                x.TotalActiveProjects,
                x.TotalProjects,
                x.Client.FirstName,
                x.Client.LastName,
                x.Client.Email,
                x.Client.Phone,
                x.Client.Address,
                x.Client.Note,
                x.Client.CreatedAtUtc,
                x.Client.UpdatedAtUtc,
                x.Client.DeletedAtUtc,
                x.Client.CreatedById,
                x.Client.UpdatedById,
                x.Client.DeletedById))
            .ToListAsync(ct);
        
        return (items, totalCount);
    }
}