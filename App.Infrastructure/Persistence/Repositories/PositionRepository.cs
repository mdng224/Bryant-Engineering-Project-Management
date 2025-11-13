using App.Application.Abstractions.Persistence.Repositories;
using App.Domain.Common.Abstractions;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public class PositionRepository(AppDbContext db) : IPositionRepository
{
    public void Add(Position position) => db.Positions.Add(position);
    
    public async Task<Position?> GetAsync(Guid id, CancellationToken ct)
    {
        var position = await db.Positions
            .AsTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id, ct);
        
        return position;
    }
    
    public async Task<Position?> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        var position = await db.Positions
            .AsTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return position;
    }
    
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