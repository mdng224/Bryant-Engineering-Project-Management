using App.Domain.Common;

namespace App.Application.Common.Dtos;

public sealed record ProjectDto(
    Guid Id,
    Guid ClientId,
    string ClientName,
    string Name,
    string Code,
    int Year,
    int Number,
    string NewCode,
    string Scope,
    string Manager,
    string Type,
    Address? Address,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    string? DeletedBy
    );