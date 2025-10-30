using App.Application.Common.Abstractions;

namespace App.Application.Users.Queries;

public sealed record GetUsersQuery(int Page, int PageSize, string? Email) : IPagedQuery;