using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Projects;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Projects.Queries.GetProjectDetails;

public class GetProjectDetailsHandler(IProjectReader projects)
    : IQueryHandler<GetProjectDetailsQuery, Result<ProjectDetailsDto>>
{
    public async Task<Result<ProjectDetailsDto>> Handle(
        GetProjectDetailsQuery query,
        CancellationToken ct)
    {
        var dto = await projects.GetDetailsAsync(query.Id, ct);

        return dto is null ? Fail<ProjectDetailsDto>("not_found", "Project not found.") : Ok(dto);
    }
}