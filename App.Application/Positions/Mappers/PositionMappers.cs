using App.Application.Positions.Commands.AddPosition;
using App.Domain.Employees;

namespace App.Application.Positions.Mappers;

public static class PositionMappers
{
    public static Position ToDomain(this AddPositionCommand command) =>
        new(command.Name,
            command.Code,
            command.RequiresLicense);
}