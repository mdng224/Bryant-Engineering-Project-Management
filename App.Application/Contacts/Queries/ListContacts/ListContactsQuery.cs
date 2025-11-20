using App.Application.Common.Pagination;

namespace App.Application.Contacts.Queries.ListContacts;

public sealed record ListContactsQuery(
    PagedQuery PagedQuery,
    string? NameFilter,
    bool? IsDeleted
);