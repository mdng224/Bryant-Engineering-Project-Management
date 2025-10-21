namespace App.Application.Common.Dtos;

public record UserDto(
    Guid Id,
    string Email,
    string RoleName,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc
);
