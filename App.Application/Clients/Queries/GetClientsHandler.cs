using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Clients.Mappers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Domain.Clients;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Clients.Queries;

public sealed class GetClientsHandler(IClientReader reader)
    : IQueryHandler<GetClientsQuery, Result<PagedResult<ClientDto>>>
{
    public async Task<Result<PagedResult<ClientDto>>> Handle(GetClientsQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = query.PagedQuery;
        var normalizedNameFilter = query.NameFilter?.ToNormalizedName();

        var (clients, total) = await reader.GetPagedAsync(
            skip,
            pageSize,
            normalizedNameFilter,
            ct);
        
        var pagedResult = new PagedResult<Client>(clients, total, page, pageSize)
            .Map(employee => employee.ToDto());
        
        return Ok(pagedResult);
    }
}