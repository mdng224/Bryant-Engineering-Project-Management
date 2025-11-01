using App.Api.Contracts.Clients;
using App.Api.Features.Clients.Mappers;
using App.Application.Abstractions.Handlers;
using App.Application.Clients.Queries;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Clients;

public static class ClientEndpoints
{
    public static void MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        var clients = app.MapGroup("/clients")
            .WithTags("Clients");

        // GET /clients?page=&pageSize=
        clients.MapGet("", HandleGetClients)
            .WithSummary("List all clients (paginated)")
            .Produces<GetClientsResponse>();
    }

    private static async Task HandleGetClients(
        [AsParameters] GetClientsRequest request,
        IQueryHandler<GetClientsQuery, Result<PagedResult<ClientDto>>> getClientsHandler,
        CancellationToken ct = default)
    {
        var getClientsQuery = request.ToQuery();
        var clientsResult  = await getClientsHandler.Handle(getClientsQuery, ct);
       // if (!clientsResult.IsSuccess)
         //   return Problem(clientsResult.Error!.Value.Message);
        
        //var response = clientsResult.Value!.ToResult();
        
        throw new NotImplementedException();
    }
}