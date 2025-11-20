namespace App.Application.Common.Dtos.Projects;

public sealed record ProjectRowDto(
    Guid   Id,
    Guid   ClientId,
    string ClientName,
    Guid   ScopeId,
    string ScopeName,
    string Name,
    string Code,
    string Manager,
    string Type,
    DateTimeOffset? DeletedAtUtc
);