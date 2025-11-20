namespace App.Application.Common.Dtos.Clients;

public sealed record ClientRowDto(
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