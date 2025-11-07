namespace App.Api.Contracts.Clients.Responses;

// Non detailed row
public sealed record ClientSummaryResponse(
    Guid Id,
    string? Name,
    string? ContactFirst,
    string? ContactMiddle,
    string? ContactLast,
    string? Email,
    string? Phone
);