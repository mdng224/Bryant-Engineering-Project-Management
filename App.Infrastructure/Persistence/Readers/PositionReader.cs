using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class PositionReader(AppDbContext db) : IPositionReader
{
        public async Task<Position?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var position = await db.Positions
            .AsTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        
        return position;
    }

    public async Task<IReadOnlyList<Position>> GetByNameIncludingDeletedAsync(
        string normalizedName,
        CancellationToken ct = default)
    {
        var positions = await db.ReadSet<Position>()
            .IgnoreQueryFilters()
            .Where(p => p.Name == normalizedName)
            .ToListAsync(ct);

        return positions;
    }
    
    public async Task<Position?> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        var position = await db.Positions
            .AsTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return position;
    }

    public async Task<(IReadOnlyList<PositionListItemDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default)
    {
        var query = db.ReadSet<Position>().ApplyDeletedFilter(isDeleted);
        
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }
        
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var items = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .Select(plid => new PositionListItemDto(
                plid.Id,
                plid.Name,
                plid.Code,
                plid.RequiresLicense,
                plid.DeletedAtUtc
                ))
            .ToListAsync(ct);
        
        return (items, totalCount);
    }
}