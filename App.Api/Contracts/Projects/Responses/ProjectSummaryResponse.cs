namespace App.Api.Contracts.Projects.Responses;

// Non detailed row
public sealed record ProjectSummaryResponse(
    Guid   Id,
    string ClientName,
    string Name,
    string NewCode,
    string Scope,
    string Manager,
    string Type
);