using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Common.Mappers;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Users.Queries;

public sealed class GetUsersHandler(IUserReader reader)
    : IQueryHandler<GetUsersQuery, Result<GetUsersResult>>
{
    public async Task<Result<GetUsersResult>> Handle(GetUsersQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = PagingDefaults.Normalize(query.Page, query.PageSize);
        var normalizedEmail = query.Email?.ToNormalizedEmail();
        
        var (users, total) = await reader.GetPagedAsync(skip, pageSize, normalizedEmail, ct);

        var userDtos = users.ToDto().ToList();
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        var result = new GetUsersResult(userDtos, total, page, pageSize, totalPages);

        return Ok(result);
    }
}
