namespace App.Api.Features.Contacts.ListContacts;

public sealed record ContactRowResponse(
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
