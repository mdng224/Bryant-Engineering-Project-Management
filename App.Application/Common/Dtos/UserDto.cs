namespace App.Application.Common.Dtos;

public sealed record UserDto(
    Guid Id,
    string Email,
    string RoleName,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc
);