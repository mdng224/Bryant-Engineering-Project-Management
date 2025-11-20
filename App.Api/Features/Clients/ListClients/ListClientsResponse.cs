namespace App.Api.Features.Clients.ListClients;

public sealed record ListClientsResponse(
    IReadOnlyList<ClientRowResponse> Clients,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);