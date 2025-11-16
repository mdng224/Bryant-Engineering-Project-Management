using App.Api.Contracts.Clients.Requests;
using App.Api.Contracts.Clients.Responses;
using App.Api.Features.Clients.Mappers;
using App.Api.Filters;
using App.Application.Abstractions.Handlers;
using App.Application.Clients.Commands.AddClient;
using App.Application.Clients.Commands.RestoreClient;
using App.Application.Clients.Queries;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Clients;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        var clients = app.MapGroup("/clients")
            .WithTags("Clients");
        
        // POST /clients
        clients.MapPost("", HandleAddClient)
            .AddEndpointFilter<Validate<AddClientRequest>>()
            .WithSummary("Create a new position")
            .Accepts<AddClientRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);
        
        // GET /clients?page=&pageSize=
        clients.MapGet("", HandleGetClients)
            .WithSummary("List all clients (paginated)")
            .Produces<GetClientsResponse>();
        
        // POST /clients/{id}/restore
        clients.MapPost("/{id:guid}/restore", HandleRestoreClient)
            .WithSummary("Restore a soft-deleted client")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> HandleAddClient(
        [FromBody] AddClientRequest request,
        [FromServices] ICommandHandler<AddClientCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
        var command = request.ToCommand();
        var result  = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return Created($"/clients/{result.Value}", result.Value);

        var error = result.Error!.Value;
        return error.Code switch
        {
            "validation" => ValidationProblem(new Dictionary<string, string[]> { ["body"] = [error.Message] }),
            "conflict"   => Conflict(new { message = error.Message }), // e.g., duplicate Code
            "forbidden"  => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            _            => Problem(error.Message)
        };
    }
    
    private static async Task<IResult> HandleGetClients(
        [AsParameters] GetClientsRequest request,
        [FromServices] IQueryHandler<GetClientsQuery, Result<PagedResult<ClientListItemDto>>> handler,
        CancellationToken ct = default)
    {
        var query = request.ToQuery();
        var result  = await handler.Handle(query, ct);
        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var response = result.Value!.ToGetClientsResponse();
        return Ok(response);
    }
    
    private static async Task<IResult> HandleRestoreClient(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<RestoreClientCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
        var command = new RestoreClientCommand(id);
        var result = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return NoContent();

        var error = result.Error!.Value;
        return error.Code switch
        {
            "not_found" => NotFound(new { message = error.Message }),
            "conflict"  => Conflict(new { message = error.Message }),   // unique-name/code taken
            "forbidden" => TypedResults.Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            _           => Problem(error.Message)
        };
    }
}