namespace App.Api.Features.Clients.ListClients;

public sealed record ClientRowResponse(
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