using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Clients.Mappers;
using App.Application.Common.Dtos.Clients.Lookups;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Clients.Queries.GetClientLookups;

public sealed class GetClientLookupsHandler(
    IClientCategoryReader categoryReader,
    IClientTypeReader typeReader)
    : IQueryHandler<GetClientLookupsQuery, Result<ClientLookupsDto>>
{
    public async Task<Result<ClientLookupsDto>> Handle(GetClientLookupsQuery query, CancellationToken ct)
    {
        var categories = await categoryReader.GetAllAsync(ct);
        var types         = await typeReader.GetAllAsync(ct);

        var dto = new ClientLookupsDto(categories.ToDtos(), types.ToDtos());

        return Ok(dto);
    }
}