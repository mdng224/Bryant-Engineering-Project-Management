using App.Api.Contracts.Positions.Requests;
using App.Api.Contracts.Positions.Responses;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Positions.Commands.AddPosition;
using App.Application.Positions.Commands.UpdatePosition;
using App.Application.Positions.Queries.GetPositions;

namespace App.Api.Features.Positions.Mappers;

public static class PositionMappers
{
    public static AddPositionCommand ToCommand(this AddProjectRequest request) =>
        new(Name: request.Name,
            Code: request.Code,
            RequiresLicense: request.RequiresLicense);

    public static UpdatePositionCommand ToCommand(this UpdateProjectRequest request,
        Guid positionId) =>
        new(PositionId: positionId,
            Name: request.Name,
            Code: request.Code,
            RequiresLicense: request.RequiresLicense);

    public static GetPositionsQuery ToQuery(this GetPositionsRequest request)
    {
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);
        var getPositionsQuery = new GetPositionsQuery(pagedQuery, request.NameFilter, request.IsDeleted);
        
        return getPositionsQuery;
    }
    
    public static GetPositionsResponse ToResponse(this PagedResult<PositionListItemDto> pagedResult) =>
        new(
            [
                .. pagedResult.Items.Select(pd => pd.ToResponse())
            ],
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize,
            pagedResult.TotalPages);

    public static PositionResponse ToResponse(this PositionListItemDto listItemDto) =>
        new(Id: listItemDto.Id,
            Name: listItemDto.Name,
            Code: listItemDto.Code,
            RequiresLicense: listItemDto.RequiresLicense,
            DeletedAtUtc: listItemDto.DeletedAtUtc);
}