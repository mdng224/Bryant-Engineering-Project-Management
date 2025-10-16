namespace App.Api.Contracts.Admins;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string RoleName,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc
);
