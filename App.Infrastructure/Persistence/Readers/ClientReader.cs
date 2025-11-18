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
        // 1. Build filtered client query
        var clientQuery = BuildClientQuery(
            normalizedNameFilter,
            hasActiveProject,
            categoryId,
            typeId);

        // 2. Count (simple query)
        var totalCount = await clientQuery.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        // 3. Decorate with joins + counts
        var categories = db.ReadSet<ClientCategory>();
        var projects = db.ReadSet<Project>().IgnoreQueryFilters();
        var types      = db.ReadSet<ClientType>();
        
        var queryWithCounts =
            from c in clientQuery
            join cat in categories on c.CategoryId equals cat.Id into catGroup
            from cat in catGroup.DefaultIfEmpty()
            join t in types on c.TypeId equals t.Id into typeGroup
            from type in typeGroup.DefaultIfEmpty()
            join p in projects on c.Id equals p.ClientId into projGroup
            from pg in projGroup.DefaultIfEmpty()
            group new { c, cat, type, pg } by new
            {
                c.Id,
                c.Name,
                c.FirstName,
                c.LastName,
                c.Email,
                c.Phone,
                c.Address,
                c.Note,
                c.CreatedAtUtc,
                c.UpdatedAtUtc,
                c.DeletedAtUtc,
                c.CreatedById,
                c.UpdatedById,
                c.DeletedById,
                CategoryName = cat != null ? cat.Name : null,
                TypeName     = type != null ? type.Name : null
            }
            into g
            select new
            {
                ClientId           = g.Key.Id,
                ClientName         = g.Key.Name,
                g.Key.FirstName,
                g.Key.LastName,
                g.Key.Email,
                g.Key.Phone,
                g.Key.Address,
                g.Key.Note,
                g.Key.CreatedAtUtc,
                g.Key.UpdatedAtUtc,
                g.Key.DeletedAtUtc,
                g.Key.CreatedById,
                g.Key.UpdatedById,
                g.Key.DeletedById,
                g.Key.CategoryName,
                g.Key.TypeName,
                TotalProjects       = g.Count(x => x.pg != null),
                TotalActiveProjects = g.Count(x => x.pg != null && x.pg.DeletedAtUtc == null)
            };
        
        // 4. Order, page, map to DTO
        var items = await queryWithCounts
            .OrderByDescending(jq => jq.TotalActiveProjects)
            .ThenByDescending(jq => jq.TotalProjects)
            .ThenBy(jq => jq.ClientName)
            .ThenBy(jq => jq.ClientId)
            .Skip(skip)
            .Take(take)
            .Select(x => new ClientListItemDto(
                x.ClientId,
                x.ClientName,
                x.TotalActiveProjects,
                x.TotalProjects,
                x.FirstName,
                x.LastName,
                x.Email,
                x.Phone,
                x.Address,
                x.Note,
                x.CategoryName,
                x.TypeName,
                x.CreatedAtUtc,
                x.UpdatedAtUtc,
                x.DeletedAtUtc,
                x.CreatedById,
                x.UpdatedById,
                x.DeletedById))
            .ToListAsync(ct);
        
        return (items, totalCount);
    }
    
    private IQueryable<Client> BuildClientQuery(
        string? normalizedNameFilter,
        bool hasActiveProject,
        Guid? categoryId,
        Guid? typeId)
    {
        var clientQuery = db.ReadSet<Client>();
        var projects    = db.ReadSet<Project>().IgnoreQueryFilters();

        // Name filter
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            clientQuery = clientQuery.Where(c =>
                EF.Functions.ILike(c.FirstName ?? "", pattern) ||
                EF.Functions.ILike(c.LastName  ?? "", pattern) ||
                EF.Functions.ILike(c.Name      ?? "", pattern));
        }

        // Category / type filters
        if (categoryId is not null)
            clientQuery = clientQuery.Where(c => c.CategoryId == categoryId);

        if (typeId is not null)
            clientQuery = clientQuery.Where(c => c.TypeId == typeId);

        // Active / inactive project filter
        clientQuery = hasActiveProject
            ? clientQuery.Where(c => projects.Any(p => p.ClientId == c.Id && p.DeletedAtUtc == null))
            : clientQuery.Where(c => !projects.Any(p => p.ClientId == c.Id && p.DeletedAtUtc == null));

        return clientQuery;
    }
}