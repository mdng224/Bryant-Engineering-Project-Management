using App.Application.Common.Dtos;
using App.Application.Positions.Queries;
using App.Domain.Employees;

namespace App.Application.Common.Mappers;

public static class PositionMapper
{
    public static GetAllPositionsResult ToResult(this IReadOnlyList<Position> positions) => new(positions.ToDto());
    
    private static List<PositionDto> ToDto(this IEnumerable<Position> positions) =>
        positions
            .Select(p => new PositionDto(p.Id, p.Name, p.Code, p.RequiresLicense))
            .ToList();
}