using App.Domain.Common;

namespace App.Api.Contracts.Clients.Responses;

public sealed record ClientDetailsResponse(
    Guid Id,
    string? Name,
    int TotalActiveProjects,
    int TotalProjects,
    string? FirstName,
    string? LastName,
    string? Email,
    string? Phone,
    Address? Address,
    string? Note,
    string? CategoryName,
    string? TypeName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    Guid? DeletedById 
    );