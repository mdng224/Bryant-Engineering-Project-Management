namespace App.Api.Contracts.Projects.Responses;

public sealed record GetProjectsResponse(
    IReadOnlyList<ProjectListItemResponse> ProjectListItemResponses,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);