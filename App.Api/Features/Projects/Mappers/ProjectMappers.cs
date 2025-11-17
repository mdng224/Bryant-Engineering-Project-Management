using App.Api.Contracts.Projects.Requests;
using App.Api.Contracts.Projects.Responses;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Projects.Queries.GetProjects;
using App.Domain.Common;

namespace App.Api.Features.Projects.Mappers;

public static class ProjectMappers
{
    public static GetProjectsResponse ToResponse(this PagedResult<ProjectListItemDto> pagedResult)
    {
        var projectListItemResponses = pagedResult.Items.Select(pd => pd.ToListItem()).ToList();

        return new GetProjectsResponse(
            ProjectListItemResponses: projectListItemResponses,
            TotalCount:               pagedResult.TotalCount,
            Page:                     pagedResult.Page,
            PageSize:                 pagedResult.PageSize,
            TotalPages:               pagedResult.TotalPages);
    }
    public static GetProjectsQuery ToQuery(this GetProjectsRequest request)
    {
        var normalizedNameFilter = (request.NameFilter ?? string.Empty).ToNormalizedName();
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);
        var getProjectsQuery = new GetProjectsQuery(pagedQuery, normalizedNameFilter, request.IsDeleted);

        return getProjectsQuery;
    }
    
    public static ProjectSummaryResponse ToSummaryResponse(this ProjectListItemDto listItemDto) =>
        new(
            Id:         listItemDto.Id,
            ClientId:   listItemDto.ClientId,
            ClientName: listItemDto.ClientName,
            ScopeId:    listItemDto.ScopeId,
            ScopeName:  listItemDto.ScopeName,
            Name:       listItemDto.Name,
            Code:       listItemDto.Code,
            Manager:    listItemDto.Manager,
            Type:       listItemDto.Type,
            DeletedAtUtc: listItemDto.DeletedAtUtc);
    
    private static ProjectListItemResponse ToListItem(this ProjectListItemDto listItemDto) =>
        new(
            Summary: listItemDto.ToSummaryResponse(),
            Details: listItemDto.ToProjectResponse());
    
    private static ProjectResponse ToProjectResponse(this ProjectListItemDto listItemDto) =>
        new(
            Id:            listItemDto.Id,
            Code:          listItemDto.Code,
            ClientId:      listItemDto.ClientId,
            ClientName:    listItemDto.ClientName,
            ScopeId:       listItemDto.ScopeId,
            ScopeName:     listItemDto.ScopeName,
            Name:          listItemDto.Name,
            Year:          listItemDto.Year,
            Number:        listItemDto.Number,
            Manager:       listItemDto.Manager,
            Type:          listItemDto.Type,
            Location:       listItemDto.Location,
            CreatedAtUtc:  listItemDto.CreatedAtUtc,
            UpdatedAtUtc:  listItemDto.UpdatedAtUtc,
            DeletedAtUtc:  listItemDto.DeletedAtUtc,
            CreatedById:   listItemDto.CreatedById,
            UpdatedById:   listItemDto.UpdatedById,
            DeletedBy: listItemDto.DeletedById == Guid.Empty
                ? "Imported From Legacy Database"
                : listItemDto.DeletedAtUtc.ToString());
}