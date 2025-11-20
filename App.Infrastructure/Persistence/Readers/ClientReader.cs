using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Clients;
using App.Domain.Clients;
using App.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class ClientReader(AppDbContext db) : IClientReader
{
    public async Task<ClientDetailsDto?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var clients    = db.ReadSet<Client>();
        var projects   = db.ReadSet<Project>().IgnoreQueryFilters();
        var categories = db.ReadSet<ClientCategory>();
        var types      = db.ReadSet<ClientType>();

        var query =
            from c in clients
            where c.Id == id
            join cat in categories on c.CategoryId equals cat.Id into catGroup
            from cat in catGroup.DefaultIfEmpty()
            join t in types on c.TypeId equals t.Id into typeGroup
            from type in typeGroup.DefaultIfEmpty()
            select new ClientDetailsDto(
                c.Id,
                c.Name,
                projects.Count(p =>
                    p.ClientId == c.Id && p.DeletedAtUtc == null),
                projects.Count(p =>
                    p.ClientId == c.Id),
                cat != null ? cat.Name : null,
                type != null ? type.Name : null,
                c.CreatedAtUtc,
                c.UpdatedAtUtc,
                c.DeletedAtUtc,
                c.CreatedById,
                c.UpdatedById,
                c.DeletedById
            );

        return await query.SingleOrDefaultAsync(ct);
    }
    
    public async Task<(IReadOnlyList<ClientRowDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter,
        bool hasActiveProject,
        Guid? categoryId,
        Guid? typeId,
        CancellationToken ct = default)
    {
        var clientQuery = BuildClientQuery(normalizedNameFilter, hasActiveProject, categoryId, typeId);

        var totalCount = await clientQuery.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var projects   = db.ReadSet<Project>().IgnoreQueryFilters();
        var categories = db.ReadSet<ClientCategory>();
        var types      = db.ReadSet<ClientType>();

        var query =
            from c in clientQuery
            join cat in categories on c.CategoryId equals cat.Id into catGroup
            from cat in catGroup.DefaultIfEmpty()
            join t in types on c.TypeId equals t.Id into typeGroup
            from type in typeGroup.DefaultIfEmpty()
            select new
            {
                c.Id,
                c.Name,
                CategoryName = cat != null ? cat.Name : null,
                TypeName     = type != null ? type.Name : null,
                TotalActiveProjects = projects.Count(p =>
                    p.ClientId == c.Id && p.DeletedAtUtc == null),
                TotalProjects = projects.Count(p =>
                    p.ClientId == c.Id)
            };
        
        var items = await query
            .OrderByDescending(jq => jq.TotalActiveProjects)
            .ThenByDescending(jq => jq.TotalProjects)
            .ThenBy(jq => jq.Name)
            .ThenBy(jq => jq.Id)
            .Skip(skip)
            .Take(take)
            .Select(x => new ClientRowDto(
                x.Id,
                x.Name,
                x.TotalActiveProjects,
                x.TotalProjects,
                x.CategoryName,
                x.TypeName))
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
            clientQuery = clientQuery.Where(c => EF.Functions.ILike(c.Name ?? "", pattern));
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