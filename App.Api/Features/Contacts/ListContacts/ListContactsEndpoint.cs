using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Contacts;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Contacts.Queries.ListContacts;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Contacts.ListContacts;

public static class ListContactsEndpoint
{
    public static RouteGroupBuilder MapListContactsEndpoint(this RouteGroupBuilder group)
    {
        // GET /contacts?page=&pageSize=
        group.MapGet("", Handle)
            .WithSummary("List contacts (paginated)")
            .Produces<ListContactsResponse>()
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [AsParameters] ListContactsRequest request,
        IQueryHandler<ListContactsQuery, Result<PagedResult<ContactRowDto>>> handler,
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

    private static ListContactsQuery ToQuery(this ListContactsRequest request)
    {
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);

        return new ListContactsQuery(pagedQuery, request.NameFilter, request.IsDeleted);
    }

    private static ListContactsResponse ToResponse(this PagedResult<ContactRowDto> pagedResult) =>
        new(
            pagedResult.Items.Select(u => u.ToResponse()).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize,
            pagedResult.TotalPages
        );
    
    private static ContactRowResponse ToResponse(this ContactRowDto dto) =>
        new(
            Id:                 dto.Id,
            ClientId:           dto.ClientId,
            FirstName:          dto.FirstName,
            LastName:           dto.LastName,
            Company:            dto.Company,
            JobTitle:           dto.JobTitle,
            Email:              dto.Email,
            BusinessPhone:      dto.BusinessPhone,
            IsPrimaryForClient: dto.IsPrimaryForClient,
            DeletedAtUtc:       dto.DeletedAtUtc
        );
}