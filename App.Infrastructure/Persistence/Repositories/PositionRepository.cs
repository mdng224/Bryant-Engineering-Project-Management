using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Writers;
using App.Domain.Common.Abstractions;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public class PositionRepository(AppDbContext db) : IPositionReader, IPositionWriter
{
    // -------------------- Readers --------------------
    public async Task<Position?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var position = await db.Positions.AsTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        
        return position;
    }

    public async Task<IReadOnlyList<Position>> GetByNameIncludingDeletedAsync(string normalizedName, CancellationToken ct = default)
    {
        var positions = await db.Positions
            .AsNoTracking()
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

    public async Task<(IReadOnlyList<Position> positions, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default)
    {
        var query = db.Positions
            .IgnoreQueryFilters()
            .AsNoTracking();
        
        query = isDeleted is true
            ? query.Where(p => p.DeletedAtUtc != null)
            : query.Where(p => p.DeletedAtUtc == null);
        
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }
        
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var positions = await query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
        
        return (positions, totalCount);
    }
    
    // -------------------- Writers --------------------
    public void Add(Position position) => db.Positions.Add(position);

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var position = await db.Positions.FindAsync([id], ct);

        switch (position)
        {
            case null:
                return false;
            case ISoftDeletable { DeletedAtUtc: not null }:
                return true;
            default:
                db.Positions.Remove(position); // interceptor flips to soft-delete
                return true;
        }
    }
    
    public void Update(Position position) => db.Positions.Update(position);
}