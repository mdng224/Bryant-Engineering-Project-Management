using App.Api.Contracts.Projects.Requests;
using App.Api.Contracts.Projects.Responses;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Projects.Queries.GetProjects;
using App.Domain.Common;

namespace App.Api.Features.Projects.Mappers;

public static class ProjectMappers
{
    public static GetProjectsQuery ToQuery(this GetProjectsRequest request)
    {
        var normalizedNameFilter = (request.NameFilter ?? string.Empty).ToNormalizedName();
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);
        var getProjectsQuery = new GetProjectsQuery(pagedQuery, normalizedNameFilter, request.IsDeleted);

        return getProjectsQuery;
    }
    
    public static ProjectResponse ToResponse(this ProjectDto dto) =>
        new(Id: dto.Id,
            Name: dto.Name,
            DeletedAtUtc: dto.DeletedAtUtc);
}