namespace App.Api.Contracts.Projects.Responses;

// Non detailed row
public sealed record ProjectSummaryResponse(
    Guid   Id,
    Guid   ClientId,
    string Name,
    string NewCode,
    string Scope,
    string Manager,
    bool   Status,
    string Type
);