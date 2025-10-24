namespace App.Application.Users.Queries;

public sealed record GetUsersQuery(int Page, int PageSize, string? Email);