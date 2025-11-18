using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Projects;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Projects.Queries.GetProjectLookups;

public sealed class GetProjectLookupsHandler(IProjectReader reader)
    : IQueryHandler<GetProjectLookupsQuery, Result<ProjectLookupsDto>>
{
    public async Task<Result<ProjectLookupsDto>> Handle(GetProjectLookupsQuery query, CancellationToken ct)
    {
        var projectManagers = await reader.GetDistinctProjectManagersAsync(ct);

        // TODO: Can add scope lookup here
        
        var dto = new ProjectLookupsDto(projectManagers);

        return Ok(dto);
    }
}