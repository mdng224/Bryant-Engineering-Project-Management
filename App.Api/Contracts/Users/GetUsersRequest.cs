namespace App.Api.Contracts.Users;

public sealed record GetUsersRequest(int Page, int PageSize, string? Email);