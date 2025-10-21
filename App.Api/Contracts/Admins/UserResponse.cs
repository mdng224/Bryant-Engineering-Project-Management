namespace App.Api.Contracts.Admins;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string RoleName,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc
);
