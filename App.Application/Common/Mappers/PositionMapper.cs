using App.Application.Common.Dtos;
using App.Application.Positions.Queries;
using App.Domain.Employees;

namespace App.Application.Common.Mappers;

public static class PositionMapper
{
    public static IEnumerable<PositionDto> ToDto(this IEnumerable<Position> positions) =>
        positions.Select(ToDto);
    private static PositionDto ToDto(this Position position) =>
        new(
            Id: position.Id,
            Name: position.Name,
            Code:  position.Code,
            RequiresLicense: position.RequiresLicense
        );

}