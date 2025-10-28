using App.Application.Abstractions;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public class PositionRepository(AppDbContext db) : IPositionReader
{
    private const int MaxPageSize = 200;
    
    // --- Readers --------------------------------------------------------
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
}