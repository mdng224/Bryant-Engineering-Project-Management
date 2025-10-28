using App.Api.Contracts.Positions;
using App.Application.Common.Dtos;
using App.Application.Positions.Queries;
using App.Application.Positions.Queries.AddPosition;
using App.Application.Positions.Queries.GetPositions;

namespace App.Api.Mappers.Positions;

public static class PositionMappers
{
    public static AddPositionCommand ToCommand(this AddPositionRequest request) =>
        new(Name: request.Name, Code:  request.Code, RequiresLicense: request.RequiresLicense);
    public static GetPositionsQuery ToQuery(this GetPositionsRequest request) =>
        new(Page: request.Page, PageSize: request.PageSize);

    public static AddPositionResponse ToResponse(this AddPositionResult result) =>
        new(Id: result.Id,
            Name: result.Name,
            Code: result.Code,
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