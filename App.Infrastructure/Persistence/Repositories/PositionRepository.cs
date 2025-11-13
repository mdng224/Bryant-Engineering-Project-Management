using App.Application.Abstractions.Persistence.Repositories;
using App.Domain.Common.Abstractions;
using App.Domain.Employees;

namespace App.Infrastructure.Persistence.Repositories;

public class PositionRepository(AppDbContext db) : IPositionRepository
{
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