namespace App.Application.Admins.Queries;

public sealed record GetUsersQuery(int Page = 1, int PageSize = 25);