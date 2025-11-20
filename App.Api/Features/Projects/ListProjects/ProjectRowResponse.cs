namespace App.Api.Features.Projects.ListProjects;

public sealed record ProjectRowResponse(
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