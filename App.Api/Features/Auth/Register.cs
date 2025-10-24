using App.Api.Contracts.Auth;
using App.Api.Mappers;
using App.Api.Mappers.Auth;
using App.Application.Abstractions;
using App.Application.Auth.Commands.Register;
using App.Application.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Auth;

public static class Register
{
    public static async Task<IResult> Handle(
        RegisterRequest request,
        ICommandHandler<RegisterCommand, Result<RegisterResult>> handler,
        CancellationToken ct)
    {
        var command = request.ToCommand();
        var result = await handler.Handle(command, ct);

        return result.ToHttpResult(registerResult =>
        {
            var registerResponse = registerResult.ToResponse();
            return Created($"/auth/users/{registerResponse.UserId}", registerResponse);
        });
    }
}