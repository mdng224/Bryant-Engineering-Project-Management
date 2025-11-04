using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Writers;
using App.Domain.Common.Abstractions;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public class PositionRepository(AppDbContext db) : IPositionReader, IPositionWriter
{
    // -------------------- Readers --------------------
    public async Task<IReadOnlyList<Position>> GetAllAsync(CancellationToken ct = default)
    {
        var positions = await db.Positions
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        return positions;
    }
    
    public async Task<(IReadOnlyList<Position> positions, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        CancellationToken ct = default)
    {
        var query = db.Positions.AsNoTracking();
        
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }
        
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var positions = await query
            .OrderBy(u => u.Name)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
        
        return (positions, totalCount);
    }
    
    // -------------------- Writers --------------------
    // TODO: Move logic to handler
    public async Task AddAsync(Position position, CancellationToken ct = default)
    {
        var matches = await db.Positions
            .IgnoreQueryFilters()
            .Where(p => p.Name == position.Name)
            .ToListAsync(ct);
        
        var activePosition = matches.FirstOrDefault(p => p.DeletedAtUtc == null);
        if (activePosition is not null)
            throw new InvalidOperationException("conflict: A position with the same name exists.");

        var tombstonePosition = matches.FirstOrDefault(p => p.DeletedAtUtc != null);
        if (tombstonePosition is not null)
        {
            tombstonePosition.ReviveAndUpdate(position.Name, position.Code, position.RequiresLicense);
            return;
        }

        await db.Positions.AddAsync(position, ct);
    }

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var position = await db.Positions.FindAsync([id], ct);

        return position switch
        {
            null => false,
            ISoftDeletable { DeletedAtUtc: not null } => true,
            _ => HandleSoftDelete(position)
        };

        bool HandleSoftDelete(Position entity)
        {
            db.Positions.Remove(entity); // interceptor flips to soft-delete
            return true;
        }
    }
    
    public async Task<Position?> GetForUpdateAsync(Guid id, CancellationToken ct) =>
        await db.Positions.FirstOrDefaultAsync(p => p.Id == id, ct);
}