using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Projects;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Projects.Queries.GetProjects;

public sealed class ListProjectsHandler(IProjectReader projectReader)
    : IQueryHandler<ListProjectsQuery, Result<PagedResult<ProjectRowDto>>>
{
    public async Task<Result<PagedResult<ProjectRowDto>>> Handle(ListProjectsQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = query.PagedQuery;
        var normalizedNameFilter = query.NameFilter?.ToNormalizedName();

        var (items, total) = await projectReader.GetPagedAsync(
            skip,
            pageSize,
            normalizedNameFilter,
            query.IsDeleted,
            query.ClientId,
            query.Manager,
            ct);

        var pagedResult = new PagedResult<ProjectRowDto>(items, total, page, pageSize);
        
        return Ok(pagedResult);
    }
}