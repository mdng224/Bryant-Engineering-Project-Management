using App.Domain.Common;

namespace App.Api.Contracts.Projects.Responses;

// For adds/updates
public record ProjectResponse(
    Guid   Id,
    Guid   ClientId,
    string ClientName,
    string Name,
    string NewCode,
    string Scope,
    string Manager,
    string Type,
    Address?  Address,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    Guid? DeletedById
);