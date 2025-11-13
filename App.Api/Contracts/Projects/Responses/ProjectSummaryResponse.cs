namespace App.Api.Contracts.Projects.Responses;

// Non detailed row
public sealed record ProjectSummaryResponse(
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