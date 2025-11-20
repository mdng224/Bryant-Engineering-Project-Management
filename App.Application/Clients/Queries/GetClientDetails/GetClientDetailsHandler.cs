using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Clients;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Clients.Queries.GetClientDetails;

public class GetClientDetailsHandler(IClientReader reader)
    : IQueryHandler<GetClientDetailsQuery, Result<ClientDetailsDto>>
{
    public async Task<Result<ClientDetailsDto>> Handle(GetClientDetailsQuery query, CancellationToken ct)
    {
        var dto = await reader.GetDetailsAsync(query.Id, ct);

        return dto is null ? Fail<ClientDetailsDto>("not_found", "Client not found.") : Ok(dto);
    }
}