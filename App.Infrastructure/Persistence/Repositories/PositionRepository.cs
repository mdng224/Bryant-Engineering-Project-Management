using App.Application.Abstractions;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public class PositionRepository(AppDbContext db) : IPositionReader
{
    // --- Readers --------------------------------------------------------
    public async Task<IReadOnlyList<Position>> GetAllAsync(CancellationToken ct = default) =>
        await db.Positions.AsNoTracking().ToListAsync(ct);
}