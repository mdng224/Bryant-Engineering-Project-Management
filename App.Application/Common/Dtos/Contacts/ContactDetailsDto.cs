using App.Domain.Contacts;

namespace App.Application.Common.Dtos.Contacts;

public sealed record ContactDetailsDto(
    Guid Id,

    // --- Client ---
    Guid? ClientId,
    
    // --- Person name ---
    string? NamePrefix,
    string FirstName,
    string? MiddleName,
    string LastName,
    string? NameSuffix,

    // --- Company / Role ---
    string? Company,
    string? Department,
    string? JobTitle,

    // --- Business Address ---
    string? AddressLine1,
    string? AddressLine2,
    string? AddressCity,
    string? AddressState,
    string? AddressPostalCode,
    string? Country,

    // --- Phones ---
    string? BusinessPhone,
    string? MobilePhone,
    PrimaryPhoneKind PrimaryPhone,

    // --- Contact info ---
    string? Email,
    string? WebPage,

    // --- Other ---
    bool IsPrimaryForClient,

    // --- Auditing ---
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    Guid? DeletedById
);