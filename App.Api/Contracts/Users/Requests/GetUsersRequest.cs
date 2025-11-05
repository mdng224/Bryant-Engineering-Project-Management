namespace App.Api.Contracts.Users.Requests;

public sealed record GetUsersRequest(int Page, int PageSize, string? EmailFilter, bool? IsDeleted);