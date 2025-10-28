using App.Api.Contracts.Positions;
using App.Application.Common.Dtos;
using App.Application.Positions.Queries;

namespace App.Api.Mappers.Positions;

public static class PositionMappers
{
    public static GetPositionsQuery ToQuery(this GetPositionsRequest request) =>
        new(Page: request.Page, PageSize: request.PageSize);
    
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