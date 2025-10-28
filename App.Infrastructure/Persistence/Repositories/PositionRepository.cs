using App.Application.Abstractions;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public class PositionRepository(AppDbContext db) : IPositionReader, IPositionWriter
{
    private const int MaxPageSize = 200;
    
    // -------------------- Readers --------------------
    public async Task<IReadOnlyList<Position>> GetAllAsync(CancellationToken ct = default)
    {
        // Stable, no-tracking read for listing and mapping
        var positions = await db.Positions
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        return positions;
    }
    
    public async Task<(IReadOnlyList<Position> positions, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, MaxPageSize);
        skip = Math.Max(0, skip);
        
        var query = db.Positions.AsNoTracking();
        
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var positions = await query
            .OrderBy(u => u.Name) // stable
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
        
        return (positions, totalCount: totalCount);
    }
    
    // -------------------- Writers --------------------
    public async Task AddAsync(Position position, CancellationToken ct = default)
        => await db.Positions.AddAsync(position, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
}