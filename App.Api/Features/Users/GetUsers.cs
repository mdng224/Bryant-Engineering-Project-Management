using App.Api.Contracts.Users;
using App.Api.Mappers.Users;
using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Users.Queries;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Users;

public static class GetUsers
{
    public static async Task<IResult> Handle(
        [AsParameters] GetUsersRequest request,
        IQueryHandler<GetUsersQuery, Result<GetUsersResult>> handler,
        CancellationToken ct)
    {
        var query = request.ToQuery();
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var response = result.Value!.ToResponse();

        return Ok(response);
    }
}