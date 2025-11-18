using App.Application.Common.Pagination;

namespace App.Application.Projects.Queries.GetProjects;

public sealed record GetProjectsQuery(
    PagedQuery PagedQuery,
    string? NameFilter,
    bool? IsDeleted,
    Guid? ClientId);
