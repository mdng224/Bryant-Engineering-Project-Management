namespace App.Api.Features.Projects.ListProjects;

public sealed record ListProjectsRequest(
    int Page,
    int PageSize,
    string? NameFilter,
    bool? IsDeleted,
    Guid? ClientId,
    string? Manager);