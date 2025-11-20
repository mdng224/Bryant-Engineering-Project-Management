using App.Api.Filters;
using App.Application.Abstractions.Handlers;
using App.Application.Clients.Commands.AddClient;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Clients.AddClient;

public static class AddClientEndpoint
{
    public static RouteGroupBuilder MapAddClientEndpoint(this RouteGroupBuilder group)
    {
        // POST /clients
        group.MapPost("", Handle)
            .AddEndpointFilter<Validate<AddClientRequest>>()
            .WithSummary("Create a new position")
            .Accepts<AddClientRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
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

    private static AddClientCommand ToCommand(this AddClientRequest request) =>
        new(
            Name:             request.Name,
            NamePrefix:       request.NamePrefix,
            FirstName:        request.FirstName,
            MiddleName:       request.MiddleName,
            LastName:         request.LastName,
            NameSuffix:       request.NameSuffix,
            Email:            request.Email,
            Phone:            request.Phone,
            Address:          request.Address,
            Note:             request.Note,
            ClientCategoryId: request.ClientCategoryId,
            ClientTypeId:     request.ClientTypeId);
}