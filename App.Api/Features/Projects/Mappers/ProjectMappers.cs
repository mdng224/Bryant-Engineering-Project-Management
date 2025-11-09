using App.Api.Contracts.Projects.Requests;
using App.Api.Contracts.Projects.Responses;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Projects.Queries.GetProjects;
using App.Domain.Common;

namespace App.Api.Features.Projects.Mappers;

public static class ProjectMappers
{
    public static GetProjectsResponse ToGetProjectsResponse(this PagedResult<ProjectDto> pagedResult)
    {
        var projects = pagedResult.Items.Select(pd => pd.ToListItem()).ToList();

        return new GetProjectsResponse(
            ProjectListItemResponses: projects,
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
    
    public static ProjectSummaryResponse ToSummaryResponse(this ProjectDto dto) =>
        new(
            Id:         dto.Id,
            ClientName: dto.ClientName,
            Name:       dto.Name,
            NewCode:    dto.NewCode,
            Scope:      dto.Scope,
            Manager:    dto.Manager,
            Type:       dto.Type);
    
    private static ProjectListItemResponse ToListItem(this ProjectDto dto) =>
        new(
            Summary: dto.ToSummaryResponse(),
            Details: dto.ToProjectResponse());
    
    private static ProjectResponse ToProjectResponse(this ProjectDto dto) =>
        new(
            Id:            dto.Id,
            ClientId:      dto.ClientId,
            ClientName:    dto.ClientName,
            Name:          dto.Name,
            NewCode:       dto.NewCode,
            Scope:         dto.Scope,
            Manager:       dto.Manager,
            Type:          dto.Type,
            Address:       dto.Address,
            CreatedAtUtc:  dto.CreatedAtUtc,
            UpdatedAtUtc:  dto.UpdatedAtUtc,
            DeletedAtUtc:  dto.DeletedAtUtc,
            CreatedById:   dto.CreatedById,
            UpdatedById:   dto.UpdatedById,
            DeletedById:   dto.DeletedById);
}