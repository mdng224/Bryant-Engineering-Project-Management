using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class EmployeePositionReader(AppDbContext db) : IEmployeePositionReader
{
    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>>> GetPositionsForEmployeesAsync(
        IReadOnlyCollection<Guid> employeeIds,
        CancellationToken ct)
    {
        if (employeeIds.Count == 0)
            return new Dictionary<Guid, IReadOnlyList<PositionMiniDto>>();
        
        // Single query; no Includes; projection only
        var rows = await db.ReadSet<EmployeePosition>()
            .Where(ep => employeeIds.Contains(ep.EmployeeId))
            .Select(ep => new
            {
                ep.EmployeeId,
                Position = new PositionMiniDto(ep.PositionId, ep.Position.Name)
            })
            .ToListAsync(ct);

        var grouped = rows.GroupBy(ep => ep.EmployeeId)
            .ToDictionary(
                g => g.Key,
                IReadOnlyList<PositionMiniDto> (g) => g.Select(ep => ep.Position)
                    .Distinct() // safety in case of dup rows
                    .ToList());

        return grouped;
    }
}