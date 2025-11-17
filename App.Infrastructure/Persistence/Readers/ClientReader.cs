using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Clients;
using App.Domain.Clients;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class ClientReader(AppDbContext db) : IClientReader
{
    public async Task<bool> EmailExistsAsync(string normalizedEmail, CancellationToken ct = default) =>
        await db.ReadSet<Client>()
            .AnyAsync(c => c.Email == normalizedEmail, ct);

    public async Task<(IReadOnlyList<ClientListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter,
        bool hasActiveProject,
        Guid? categoryId,
        Guid? typeId,
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
        
        if (categoryId is not null)
            clientQuery = clientQuery.Where(c => c.CategoryId == categoryId);
        if (typeId is not null)
            clientQuery = clientQuery.Where(c => c.TypeId == typeId);

        var queryWithCounts =
            from c in clientQuery
            join cat in db.ReadSet<ClientCategory>() on c.CategoryId equals (Guid?)cat.Id into catGroup
            from cat in catGroup.DefaultIfEmpty()
            join t in db.ReadSet<ClientType>() on c.TypeId equals (Guid?)t.Id into typeGroup
            from type in typeGroup.DefaultIfEmpty()
            join p in db.ReadSet<Project>().IgnoreQueryFilters()
                on c.Id equals p.ClientId into projGroup
            from pg in projGroup.DefaultIfEmpty()
            group new { c, cat, type, pg } by new
            {
                Client      = c,
                CategoryName = cat != null ? cat.Name : null,
                TypeName     = type != null ? type.Name : null
            }
            into g
            select new
            {
                g.Key.Client,
                g.Key.CategoryName,
                g.Key.TypeName,
                TotalProjects = g.Count(x => x.pg != null),
                TotalActiveProjects = g.Count(x => x.pg != null && x.pg.DeletedAtUtc == null)
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
                x.CategoryName,
                x.TypeName,
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