using App.Application.Common.Pagination;

namespace App.Application.Users.Queries;

public sealed record GetUsersQuery(PagedQuery PagedQuery, string? EmailFilter);