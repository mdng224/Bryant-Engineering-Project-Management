using App.Application.Common.Dtos;
using App.Application.Positions.Commands.AddPosition;
using App.Domain.Employees;

namespace App.Application.Positions.Mappers;

public static class PositionMappers
{
    public static Position ToDomain(this AddPositionCommand command) =>
        new(command.Name,
            command.Code,
            command.RequiresLicense);
    
    public static PositionListItemDto ToDto(this Position position) =>
        new(
            Id: position.Id,
            Name: position.Name,
            Code:  position.Code,
            RequiresLicense: position.RequiresLicense,
            DeletedAtUtc: position.DeletedAtUtc
        );
}