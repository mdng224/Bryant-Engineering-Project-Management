using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Projects.Mappers;
using App.Domain.Common;
using App.Domain.Projects;
using static App.Application.Common.R;

namespace App.Application.Projects.Queries.GetProjects;

public sealed class GetProjectsHandler(IProjectReader reader)
    : IQueryHandler<GetProjectsQuery, Result<PagedResult<ProjectDto>>>
{
    public async Task<Result<PagedResult<ProjectDto>>> Handle(GetProjectsQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = query.PagedQuery;
        var normalizedNameFilter = query.NameFilter?.ToNormalizedName();
        var (projects, total) = await reader.GetPagedAsync(skip,
            pageSize,
            normalizedNameFilter,
            query.IsDeleted,
            ct);

        var pagedResult = new PagedResult<Project>(projects, total, page, pageSize)
            .Map(project => project.ToDto());
        
        return Ok(pagedResult);
    }
}