namespace App.Application.Common.Dtos.Contacts;

public sealed record ContactRowDto(
    Guid Id,
    Guid? ClientId,
    string FirstName,
    string LastName,
    string? Company,
    string? JobTitle,
    string? Email,
    string? BusinessPhone,
    bool IsPrimaryForClient,
    DateTimeOffset? DeletedAtUtc
);