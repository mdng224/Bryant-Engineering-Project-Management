using App.Application.Common.Dtos;
using App.Application.Positions.Commands.AddPosition;
using App.Application.Positions.Commands.UpdatePosition;
using App.Application.Positions.Queries;
using App.Domain.Employees;

namespace App.Application.Common.Mappers;

public static class PositionMapper
{
    public static Position ToDomain(this AddPositionCommand command) =>
        new(command.Name,
            command.Code,
            command.RequiresLicense);
    
    public static IEnumerable<PositionDto> ToDto(this IEnumerable<Position> positions) =>
        positions.Select(ToDto);

    public static PositionResult ToResult(this Position position) =>
        new(Id: position.Id,
            Name: position.Name,
            Code: position.Code ?? string.Empty,
            RequiresLicense: position.RequiresLicense);
    
    private static PositionDto ToDto(this Position position) =>
        new(
            Id: position.Id,
            Name: position.Name,
            Code:  position.Code,
            RequiresLicense: position.RequiresLicense
        );
}