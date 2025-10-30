using App.Api.Common.Pagination;

namespace App.Api.Contracts.Users;

public sealed record GetUsersRequest(PagedRequest PagedRequest, string? Email);