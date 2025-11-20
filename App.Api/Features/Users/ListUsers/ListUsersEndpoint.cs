using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Users.Queries;
using App.Domain.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Users.ListUsers;

public static class ListUsersEndpoint
{
    public static RouteGroupBuilder MapListUsersEndpoint(this RouteGroupBuilder group)
    {
        // GET /users?page=&pageSize=
        group.MapGet("", Handle)
            .WithSummary("List users (paginated)")
            .Produces<ListUsersResponse>()
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [AsParameters] ListUsersRequest request,
        IQueryHandler<GetUsersQuery, Result<PagedResult<UserDto>>> handler,
        CancellationToken ct)
    {
        var query = request.ToQuery();
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
        {
            var e = result.Error!.Value;
            return e.Code switch
            {
                "validation" => ValidationProblem(new Dictionary<string, string[]> { ["query"] = [e.Message] }),
                "forbidden"  => Json(new { message = e.Message }, statusCode: StatusCodes.Status403Forbidden),
                _            => Problem(e.Message)
            };
        }

        var response = result.Value!.ToResponse();
        return Ok(response);
    }

    private static GetUsersQuery ToQuery(this ListUsersRequest request)
    {
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);
        var normalizedEmail = request.EmailFilter?.ToNormalizedEmail();

        return new GetUsersQuery(pagedQuery, normalizedEmail, request.IsDeleted);
    }

    private static ListUsersResponse ToResponse(this PagedResult<UserDto> pagedResult) =>
        new(
            pagedResult.Items.Select(u => u.ToResponse()).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize,
            pagedResult.TotalPages
        );
    
    private static UserRowResponse ToResponse(this UserDto dto) =>
        new(
            Id: dto.Id,
            Email: dto.Email,
            RoleName: dto.RoleName,
            Status: dto.Status.ToString(),
            CreatedAtUtc: dto.CreatedAtUtc,
            UpdatedAtUtc: dto.UpdatedAtUtc,
            DeletedAtUtc: dto.DeletedAtUtc
        );
}