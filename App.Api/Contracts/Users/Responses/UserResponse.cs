namespace App.Api.Contracts.Users.Responses;

public sealed record UserResponse(
    Guid Id,
    string Email,
    string RoleName,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc
);
