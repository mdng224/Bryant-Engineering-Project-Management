using App.Domain.Common;

namespace App.Api.Features.Clients.GetClientDetails;

public sealed record GetClientDetailsResponse(
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