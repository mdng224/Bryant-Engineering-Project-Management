using App.Application.Abstractions.Persistence;
using App.Application.Common.Dtos;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class EmployeePositionRepository(AppDbContext db) : IEmployeePositionReader
{
    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>>> GetPositionsForEmployeesAsync(
        IReadOnlyCollection<Guid> employeeIds,
        CancellationToken ct)
    {
        if (employeeIds.Count == 0)
            return new Dictionary<Guid, IReadOnlyList<PositionMiniDto>>();
        
        // Single query; no Includes; projection only
        var rows = await db.EmployeePositions
            .AsNoTracking()
            .Where(ep => employeeIds.Contains(ep.EmployeeId))
            .Select(ep => new
            {
                ep.EmployeeId,
                Position = new PositionMiniDto(ep.PositionId, ep.Position!.Name)
            })
            .ToListAsync(ct);

        var grouped = rows
            .GroupBy(x => x.EmployeeId)
            .ToDictionary(
                g => g.Key, IReadOnlyList<PositionMiniDto> (g) => g
                    .Select(x => x.Position)
                    .Distinct() // safety in case of dup rows
                    .ToList());

        return grouped;
    }
}