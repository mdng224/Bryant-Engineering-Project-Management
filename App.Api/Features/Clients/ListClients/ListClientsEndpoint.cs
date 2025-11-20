using App.Application.Abstractions.Handlers;
using App.Application.Clients.Queries.ListClients;
using App.Application.Common.Dtos.Clients;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Clients.ListClients;

public static class ListClientsEndpoint
{
    public static RouteGroupBuilder MapListClientsEndpoint(this RouteGroupBuilder group)
    {
        // GET /clients?page=&pageSize=
        group.MapGet("", Handle)
            .WithSummary("List all clients (paginated)")
            .Produces<ListClientsResponse>();

        return group;
    }
    
    private static async Task<IResult> Handle(
        [AsParameters] ListClientsRequest request,
        [FromServices] IQueryHandler<ListClientsQuery, Result<PagedResult<ClientRowDto>>> handler,
        CancellationToken ct = default)
    {
        var query = request.ToQuery();
        var result  = await handler.Handle(query, ct);
        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var response = result.Value!.ToResponse();
        return Ok(response);
    }

    private static ListClientsQuery ToQuery(this ListClientsRequest request)
    {
        var pagedQuery      = new PagedQuery(request.Page, request.PageSize);
        var getClientsQuery = new ListClientsQuery(
            pagedQuery,
            request.NameFilter,
            request.HasActiveProject,
            request.CategoryId,
            request.TypeId);

        return getClientsQuery;
    }

    private static ListClientsResponse ToResponse(this PagedResult<ClientRowDto> pagedResult)
    {
        var clients = pagedResult.Items.Select(dto => dto.ToResponse()).ToList();
        
        return new ListClientsResponse(
            Clients:    clients,
            TotalCount: pagedResult.TotalCount,
            Page:       pagedResult.Page,
            PageSize:   pagedResult.PageSize,
            TotalPages: pagedResult.TotalPages
        );
    }
    
    private static ClientRowResponse ToResponse(this ClientRowDto dto) =>
        new(
            Id:                  dto.Id,
            Name:                dto.Name,
            TotalActiveProjects: dto.TotalActiveProjects,
            TotalProjects:       dto.TotalProjects,
            FirstName:           dto.FirstName,
            LastName:            dto.LastName,
            Email:               dto.Email,
            Phone:               dto.Phone,
            CategoryName:        dto.CategoryName,
            TypeName:            dto.TypeName);


}