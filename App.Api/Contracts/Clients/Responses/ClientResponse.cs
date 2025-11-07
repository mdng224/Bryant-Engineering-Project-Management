using App.Domain.Common;

namespace App.Api.Contracts.Clients.Responses;

public sealed record ClientResponse(
    Guid Id,
    string? Name,
    string? ContactFirst,
    string? ContactMiddle,
    string? ContactLast,
    string? Email,
    string? Phone,
    Address? Address,
    string? Note,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    Guid? DeletedById 
    );