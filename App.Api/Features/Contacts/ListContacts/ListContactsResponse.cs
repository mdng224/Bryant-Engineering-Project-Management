namespace App.Api.Features.Contacts.ListContacts;

public sealed record ListContactsResponse(
    IReadOnlyList<ContactRowResponse> Contacts,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
