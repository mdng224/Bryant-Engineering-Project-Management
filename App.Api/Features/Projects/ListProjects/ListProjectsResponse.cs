namespace App.Api.Features.Projects.ListProjects;

public sealed record ListProjectsResponse(
    IReadOnlyList<ProjectRowResponse> Projects,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);