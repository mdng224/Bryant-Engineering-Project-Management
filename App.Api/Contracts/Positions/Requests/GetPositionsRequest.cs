namespace App.Api.Contracts.Positions.Requests;

public sealed record GetPositionsRequest(int Page, int PageSize, string? NameFilter, bool? IsDeleted);