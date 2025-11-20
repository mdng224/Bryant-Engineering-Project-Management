using App.Api.Common.Mappers;
using App.Api.Filters;
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
            .AddEndpointFilter<Validate<RegisterRequest>>()
            .Accepts<RegisterRequest>("application/json")
            .WithName("Auth_Register")
            .WithSummary("Register a new user")
            .WithDescription("Create a new user account with email and password.")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        return group;
    }
    
    private static async Task<IResult> HandleRegister(
        [FromBody] RegisterRequest request,
        [FromServices] ICommandHandler<RegisterCommand, Result<RegisterResult>> handler,
        CancellationToken ct)
    {
        var command = new RegisterCommand(request.Email, request.Password);
        var result  = await handler.Handle(command, ct);

        
        return result.ToHttpResult(registerResult =>
        {
            var registerResponse = new RegisterResponse(registerResult.UserId,
                "pending",
                "Your account is pending administrator.");
            return Created($"/auth/users/{registerResponse.UserId}", registerResponse);
        });
    }
}