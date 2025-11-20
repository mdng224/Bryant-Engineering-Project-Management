using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Projects;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Projects.Queries.GetProjects;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Projects.ListProjects;

public static class ListProjectsEndpoint
{
    public static RouteGroupBuilder MapListProjectsEndpoint(this RouteGroupBuilder group)
    {
        // GET /projects?page=&pageSize=
        group.MapGet("", Handle)
            .WithSummary("List all projects (paginated)")
            .Produces<ListProjectsResponse>();

        return group;
    }
    
    private static async Task<IResult> Handle(
        [AsParameters] ListProjectsRequest request,
        [FromServices] IQueryHandler<ListProjectsQuery, Result<PagedResult<ProjectRowDto>>> handler,
        CancellationToken ct = default)
    {
        var query = request.ToQuery();
        var result  = await handler.Handle(query, ct);
        
        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var dto = result.Value!;
        var response = dto.ToResponse();

        return Ok(response);
    }

    private static ListProjectsQuery ToQuery(this ListProjectsRequest request)
    {
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);
        
        return new ListProjectsQuery(
            PagedQuery: pagedQuery,
            NameFilter: request.NameFilter,
            IsDeleted:  request.IsDeleted,
            ClientId:   request.ClientId,
            Manager:    request.Manager
        );
    }

    private static ListProjectsResponse ToResponse(this PagedResult<ProjectRowDto> pagedResult)
    {
        var projects = pagedResult.Items.Select(dto => dto.ToResponse()).ToList();
        
        return new ListProjectsResponse(
            Projects:   projects,
            TotalCount: pagedResult.TotalCount,
            Page:       pagedResult.Page,
            PageSize:   pagedResult.PageSize,
            TotalPages: pagedResult.TotalPages
        );
    }

    private static ProjectRowResponse ToResponse(this ProjectRowDto dto) =>
        new(
            Id:           dto.Id,
            ClientId:     dto.ClientId,
            ClientName:   dto.ClientName,
            ScopeId:      dto.ScopeId,
            ScopeName:    dto.ScopeName,
            Name:         dto.Name,
            Code:         dto.Code,
            Manager:      dto.Manager,
            Type:         dto.Type,
            DeletedAtUtc: dto.DeletedAtUtc
        );
}