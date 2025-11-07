namespace App.Api.Contracts.Projects.Requests;

public sealed record GetProjectsRequest(int Page, int PageSize, string? NameFilter, bool? IsDeleted);