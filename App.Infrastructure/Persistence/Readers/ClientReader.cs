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
        var clientQuery = db.ReadSet<Client>().ApplyDeletedFilter(isDeleted);
        
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            clientQuery = clientQuery.Where(c =>
                EF.Functions.ILike(c.FirstName ?? "", pattern) ||
                EF.Functions.ILike(c.LastName  ?? "", pattern) ||
                EF.Functions.ILike(c.Name      ?? "", pattern));
        }

        var totalCount = await clientQuery.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var projectQuery = db.ReadSet<Project>();
        var queryWithCounts = clientQuery
            .Select(c => new
            {
                Client = c,
                TotalActiveProjects = projectQuery.Count(p => p.ClientId == c.Id),
                TotalProjects       = projectQuery.IgnoreQueryFilters().Count(p => p.ClientId == c.Id)
            });
        
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