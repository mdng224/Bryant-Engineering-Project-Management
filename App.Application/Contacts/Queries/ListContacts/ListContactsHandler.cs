using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Contacts;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Contacts.Queries.ListContacts;

public sealed class ListContactsHandler(IContactReader reader)
    : IQueryHandler<ListContactsQuery, Result<PagedResult<ContactRowDto>>>
{
    public async Task<Result<PagedResult<ContactRowDto>>> Handle(ListContactsQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = query.PagedQuery;

        var (items, total) = await reader.GetPagedAsync(
            skip,
            pageSize,
            query.NameFilter,
            query.IsDeleted,
            ct);
        
        var pagedResult = new PagedResult<ContactRowDto>(items, total, page, pageSize);

        return Ok(pagedResult);
    }
}