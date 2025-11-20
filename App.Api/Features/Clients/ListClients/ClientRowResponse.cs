namespace App.Api.Features.Clients.ListClients;

public sealed record ClientRowResponse(
    Guid     Id,
    string? Name,
    int     TotalActiveProjects,
    int     TotalProjects,
    string? CategoryName,
    string? TypeName
);