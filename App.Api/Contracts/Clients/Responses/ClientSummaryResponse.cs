namespace App.Api.Contracts.Clients.Responses;

// Non detailed row
public sealed record ClientSummaryResponse(
    Guid     Id,
    string? Name,
    int     TotalActiveProjects,
    int     TotalProjects,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    string? CategoryName,
    string? TypeName
);