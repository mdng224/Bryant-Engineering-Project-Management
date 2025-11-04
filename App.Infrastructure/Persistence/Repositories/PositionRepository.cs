using System.Reflection.Metadata;
using App.Application.Abstractions.Persistence;
using App.Domain.Common.Abstractions;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
    public async Task AddAsync(Position position, CancellationToken ct = default)
    {
        var nameExists = await db.Positions.AnyAsync(p => p.Name == position.Name, ct);
        
        if (nameExists)
            throw new InvalidOperationException("conflict: A position with the same Name exists.");

        var tombstone = await db.Positions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Name == position.Name && p.DeletedAtUtc != null, ct);

        if (tombstone is null)
        {
            await db.Positions.AddAsync(position, ct);
            return;
        }

        tombstone.ReviveAndUpdate(position.Name, position.Code, position.RequiresLicense);
    }

    public async Task<bool> SoftDeleteAsync(Guid positionId, CancellationToken ct = default)
    {
        var position = await db.Positions.FindAsync([positionId], ct);

        return position switch
        {
            null => false,
            ISoftDeletable { DeletedAtUtc: not null } => true,
            _ => await HandleSoftDeleteAsync(position, ct)
        };
        
        async Task<bool> HandleSoftDeleteAsync(Position entity, CancellationToken token)
        {
            db.Positions.Remove(entity);      // interceptor flips to soft-delete
            await SaveChangesAsync(token);
            return true;
        }
    }
    
    public async Task<Position?> GetForUpdateAsync(Guid positionId, CancellationToken ct) =>
        await db.Positions.FirstOrDefaultAsync(p => p.Id == positionId, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}