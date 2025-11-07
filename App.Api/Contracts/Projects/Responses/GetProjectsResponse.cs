namespace App.Api.Contracts.Projects.Responses;

public sealed record GetProjectsResponse(
    IReadOnlyList<ProjectResponse> Positions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);