using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Common.Mappers;
using static App.Application.Common.R;

namespace App.Application.Admins.Queries;

public sealed class GetUsersHandler(IUserReader userReader)
    : IQueryHandler<GetUsersQuery, Result<GetUsersResult>>
{
    private const int MaxPageSize = 200;

    public async Task<Result<GetUsersResult>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        // normalize paging (defensive)
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 25 : Math.Min(query.PageSize, MaxPageSize);
        var skip = (page - 1) * pageSize;

        var (users, total) = await userReader.GetPagedAsync(skip, pageSize, ct);

        var items = users.ToDto().ToList();
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        var getUsersResult = new GetUsersResult(items, total, page, pageSize, totalPages);

        return Ok(getUsersResult);
    }
}
