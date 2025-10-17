using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Common.Mappers;
using static App.Application.Common.R;

namespace App.Application.Admins.Queries;

public sealed class GetUsersHandler(IUserReader userReader)
    : IQueryHandler<GetUsersQuery, Result<GetUsersResult>>
{
    public async Task<Result<GetUsersResult>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = PagingDefaults.Normalize(query.Page, query.PageSize);

        var (users, total) = await userReader.GetPagedAsync(skip, pageSize, query.Email, ct);

        var items = users.ToDto().ToList();
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        var getUsersResult = new GetUsersResult(items, total, page, pageSize, totalPages);

        return Ok(getUsersResult);
    }
}
