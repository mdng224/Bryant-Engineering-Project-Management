using App.Domain.Common;

namespace App.Api.Contracts.Projects.Responses;

// For adds/updates
public record ProjectResponse(
    Guid   Id,
    Guid   ClientId,
    string Name,
    string NewCode,
    string Scope,
    string Manager,
    bool   Status,
    string Type,
    Address?  Address,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    Guid? DeletedById
);