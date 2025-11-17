using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Clients;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Clients.Queries.GetClients;

public sealed class GetClientsHandler(IClientReader clientReader)
    : IQueryHandler<GetClientsQuery, Result<PagedResult<ClientListItemDto>>>
{
    public async Task<Result<PagedResult<ClientListItemDto>>> Handle(GetClientsQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = query.PagedQuery;
        var normalized = query.NameFilter?.ToNormalizedName();

        var (items, total) = await clientReader.GetPagedAsync(
            skip,
            pageSize,
            normalized,
            query.HasActiveProject,
            query.CategoryId,
            query.TypeId,
            ct);
        
        var pagedResult = new PagedResult<ClientListItemDto>(items, total, page, pageSize);

        return Ok(pagedResult);
    }
}