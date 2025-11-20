namespace App.Application.Common.Dtos.Clients;

public sealed record ClientDetailsDto(
    Guid Id,
    string Name,
    int TotalActiveProjects,
    int TotalProjects,
    string? CategoryName,
    string? TypeName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    Guid? DeletedById);