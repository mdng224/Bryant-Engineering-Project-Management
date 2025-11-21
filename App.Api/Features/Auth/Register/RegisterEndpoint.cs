using App.Application.Abstractions.Handlers;
using App.Application.Auth.Commands.Register;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Auth.Register;

public static class RegisterEndpoint
{
    public static RouteGroupBuilder MapRegisterEndpoint(this RouteGroupBuilder group)
    {
        // ---- POST /auth/register
        group.MapPost("/register", HandleRegister)
            .AllowAnonymous()
            .Accepts<RegisterRequest>("application/json")
            .WithName("Auth_Register")
            .WithSummary("Register a new user")
            .WithDescription("Create a new user account with email and password.")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status409Conflict);

        return group;
    }
    
    private static async Task<IResult> HandleRegister(
        [FromBody] RegisterRequest request,
        [FromServices] ICommandHandler<RegisterCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
        var command = new RegisterCommand(request.Email, request.Password);
        var result  = await handler.Handle(command, ct);
        
        if (result.IsSuccess)
            return Created($"~/users/{result.Value}", result.Value);

        var error = result.Error!.Value;
        return error.Code switch
        {
            "validation" => ValidationProblem(
                errors: new Dictionary<string, string[]> { ["body"] = [error.Message] }),
            "conflict"   => Conflict(new { message = error.Message }),
            _            => Problem(error.Message)
        };
    }
}