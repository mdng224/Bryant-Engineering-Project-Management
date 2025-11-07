namespace App.Api.Contracts.Projects.Responses;

// For adds/updates
public record ProjectResponse(
    Guid Id,
    string Name,
    DateTimeOffset? DeletedAtUtc);