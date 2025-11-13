using App.Domain.Users;

namespace App.Application.Common.Dtos;

public record UserDto(
    Guid Id,
    string Email,
    string RoleName,
    UserStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc
);
