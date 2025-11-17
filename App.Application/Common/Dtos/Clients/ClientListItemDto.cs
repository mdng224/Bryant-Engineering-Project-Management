using App.Domain.Common;

namespace App.Application.Common.Dtos.Clients;

public sealed record ClientListItemDto(
    Guid Id,
    string Name,
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
    Guid? DeletedById);