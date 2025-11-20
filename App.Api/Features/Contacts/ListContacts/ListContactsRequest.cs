namespace App.Api.Features.Contacts.ListContacts;

public sealed record ListContactsRequest(
    int Page,
    int PageSize,
    string? NameFilter,
    bool? IsDeleted
);