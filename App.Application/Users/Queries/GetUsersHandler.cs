using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Domain.Common;
using App.Domain.Users;
using static App.Application.Common.R;

namespace App.Application.Users.Queries;

public sealed class GetUsersHandler(IUserReader reader) : IQueryHandler<GetUsersQuery, Result<PagedResult<UserDto>>>
{
    public async Task<Result<PagedResult<UserDto>>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = query.PagedQuery;
        var normalizedEmailFilter = query.EmailFilter?.ToNormalizedEmail();

        var (users, total) = await reader.GetPagedAsync(skip,
            pageSize,
            normalizedEmailFilter,
            ct);

        var pagedResult = new PagedResult<User>(users, total, page, pageSize)
            .Map(user => user.ToDto());

        return Ok(pagedResult);
    }
}
