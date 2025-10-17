namespace App.Api.Contracts.Admins;

public sealed record GetUsersRequest(int Page, int PageSize, string? Email);
