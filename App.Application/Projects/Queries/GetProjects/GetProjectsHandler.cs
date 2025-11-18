using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Dtos.Projects;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Projects.Queries.GetProjects;

public sealed class GetProjectsHandler(IProjectReader projectReader)
    : IQueryHandler<GetProjectsQuery, Result<PagedResult<ProjectListItemDto>>>
{
    public async Task<Result<PagedResult<ProjectListItemDto>>> Handle(GetProjectsQuery query, CancellationToken ct)
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

        var pagedResult = new PagedResult<ProjectListItemDto>(items, total, page, pageSize);
        
        return Ok(pagedResult);
    }
}