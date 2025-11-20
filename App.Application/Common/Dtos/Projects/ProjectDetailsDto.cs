namespace App.Application.Common.Dtos.Projects;

public sealed record ProjectDetailsDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    Guid ScopeId,
    string ScopeName,
    string Name,
    string Code,
    int Year,
    int Number,
    string Manager,
    string Type,
    string Location,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    Guid? DeletedById
);