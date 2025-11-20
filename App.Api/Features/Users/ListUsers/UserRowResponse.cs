namespace App.Api.Features.Users.ListUsers;

public sealed record UserRowResponse(
    Guid Id,
    string Email,
    string RoleName,
    string Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc
);
