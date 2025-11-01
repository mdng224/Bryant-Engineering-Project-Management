using App.Application.Abstractions;
using App.Application.Abstractions.Persistence;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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
        string? normalizedNameFilter = null,
        CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, MaxPageSize);
        skip = Math.Max(0, skip);
        
        var query = db.Positions.AsNoTracking();
        // check last, first, and nickname
        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }
        
        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);
        
        var positions = await query
            .OrderBy(u => u.Name) // stable
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);
        
        return (positions, totalCount);
    }
    
    // -------------------- Writers --------------------
    public async Task AddAsync(Position position, CancellationToken ct = default)
        => await db.Positions.AddAsync(position, ct);

    public async Task<bool> DeleteAsync(Guid positionId, CancellationToken ct = default)
    {
        var position = await db.Positions.FindAsync([positionId], ct);
        if (position is null)
            return false;
        
        db.Positions.Remove(position);
        try
        {
            await SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: "23503" })
        {
            // Position is referenced elsewhere (e.g., by Employees)
            // Decide how you want to surface this (throw custom, return false, etc.)
            // Here I’ll throw so the API can map to 409 Conflict.
            throw new InvalidOperationException("position_in_use: Position is referenced and cannot be deleted.", ex);
        }
    }
    
    public Task<Position?> GetForUpdateAsync(Guid positionId, CancellationToken ct) =>
        db.Positions.FindAsync([positionId], ct).AsTask(); // tracked
    
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => db.SaveChangesAsync(ct);
    
    public async Task<bool> UpdateAsync(Position position, CancellationToken ct = default)
    {
        var existingPosition = await db.Positions.FindAsync([position.Id], ct);
        if (existingPosition is null)
            return false;

        existingPosition.Rename(position.Name);
        existingPosition.SetCode(position.Code);
        existingPosition.RequireLicense(position.RequiresLicense);

        try
        {
            await SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pg)
        {
            // Handle Postgres constraint violations gracefully
            return pg.SqlState switch
            {
                "23505" => throw new InvalidOperationException(
                    "duplicate_entry: A position with the same name or code already exists.", ex),
                _ => throw new InvalidOperationException(
                    $"database_error: Unexpected database error (SQL state {pg.SqlState}).", ex)
            };
        }
    }
}