using App.Api.Contracts.Auth;
using App.Api.Mappers;
using App.Api.Mappers.Auth;
using App.Application.Abstractions;
using App.Application.Auth.Queries.Login;
using App.Application.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Auth;

public static class Login
{
    public static async Task<IResult> Handle(
        LoginRequest request,
        IQueryHandler<LoginQuery, Result<LoginResult>> handler,
        CancellationToken ct)
    {
        var query = request.ToQuery();
        var result = await handler.Handle(query, ct);

        return result.ToHttpResult(loginResult => Ok(loginResult.ToResponse()));
    }
}