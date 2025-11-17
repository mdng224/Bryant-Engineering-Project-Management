using App.Api.Contracts.Clients.Responses.Lookups;
using App.Api.Features.Clients.Mappers;
using App.Application.Abstractions.Handlers;
using App.Application.Clients.Queries.GetClientLookups;
using App.Application.Common.Dtos.Clients.Lookups;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Clients;

public static class ClientLookupEndpoints
{
    public static void MapClientLookupEndpoints(this IEndpointRouteBuilder app)
    {
        var lookups = app.MapGroup("/clients/lookups")
            .WithTags("Client lookups");
        
        // GET /clients/lookups
        lookups.MapGet("", HandleGetClientLookups)
            .WithSummary("Get lookup data for client categories and client types")
            .Produces<ClientLookupsResponse>();
    }
    
    private static async Task<IResult> HandleGetClientLookups(
        [FromServices] IQueryHandler<GetClientLookupsQuery, Result<ClientLookupsDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetClientLookupsQuery(), ct);

        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var dto = result.Value!;

        var response = new ClientLookupsResponse(
            Categories: dto.Categories.ToResponses(),
            Types: dto.Types.ToResponses()
        );

        return Ok(response);
    }
}