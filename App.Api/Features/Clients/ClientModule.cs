using App.Api.Features.Clients.AddClient;
using App.Api.Features.Clients.GetClientDetails;
using App.Api.Features.Clients.ListClientLookups;
using App.Api.Features.Clients.ListClients;
using App.Api.Features.Clients.RestoreClient;
using App.Api.Features.Contacts.GetContactDetails;

namespace App.Api.Features.Clients;

public static class ClientModule
{
    public static void MapClientEndpoints(this IEndpointRouteBuilder app)
    {
        var clients = app.MapGroup("/clients")
            .WithTags("Clients");
        
        clients.MapAddClientEndpoint();
        clients.MapGetClientDetailsEndpoint();
        clients.MapListClientLookupsEndpoint();
        clients.MapListClientsEndpoint();
        clients.MapRestoreClientEndpoint();
    }
}