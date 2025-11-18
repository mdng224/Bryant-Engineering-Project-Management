namespace App.Api.Contracts.Clients.Responses;

// One row in the table
public sealed record ClientListItemResponse(
    ClientSummaryResponse ClientSummaryResponse,
    ClientDetailsResponse ClientDetailsResponse
);