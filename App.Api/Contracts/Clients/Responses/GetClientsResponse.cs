namespace App.Api.Contracts.Clients.Responses;

public sealed record GetClientsResponse(
    IReadOnlyList<ClientListItemResponse> ClientListItemResponses,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);