using App.Api.Contracts.Positions;
using App.Application.Common.Dtos;
using App.Application.Positions.Commands.AddPosition;
using App.Application.Positions.Commands.UpdatePosition;
using App.Application.Positions.Queries.GetPositions;

namespace App.Api.Features.Positions.Mappers;

public static class PositionMappers
{
    public static AddPositionCommand ToCommand(this AddPositionRequest request) =>
        new(Name: request.Name,
            Code: request.Code,
            RequiresLicense: request.RequiresLicense);

    public static UpdatePositionCommand ToCommand(this UpdatePositionRequest request,
        Guid positionId) =>
        new(PositionId: positionId,
            Name: request.Name,
            Code: request.Code,
            RequiresLicense: request.RequiresLicense);
    
    public static GetPositionsQuery ToQuery(this GetPositionsRequest request) =>
        new(Page: request.Page, PageSize: request.PageSize);

    public static PositionResponse ToResponse(this PositionResult result) =>
        new(Id: result.Id,
            Name: result.Name,
            Code: result.Code ?? string.Empty,
            RequiresLicense: result.RequiresLicense);
    
    public static GetPositionsResponse ToResponse(this GetPositionsResult result) =>
        new(
            [
                .. result.Positions.Select(pd => pd.ToResponse())
            ],
            result.TotalCount,
            result.Page,
            result.PageSize,
            result.TotalPages);

    private static PositionResponse ToResponse(this PositionDto dto) =>
        new(Id: dto.Id, Name: dto.Name, Code: dto.Code, RequiresLicense: dto.RequiresLicense);
}