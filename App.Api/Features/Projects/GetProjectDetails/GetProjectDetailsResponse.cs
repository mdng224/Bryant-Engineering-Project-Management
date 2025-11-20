namespace App.Api.Features.Projects.GetProjectDetails;

public sealed record GetProjectDetailsResponse(
    Guid   Id,
    string Code,
    Guid   ClientId,
    string ClientName,
    Guid   ScopeId,
    string ScopeName,
    string Name,
    int Year,
    int Number,
    string Manager,
    string Type,
    string  Location,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    string? DeletedBy);