using App.Api.Features.Positions.RestorePosition;
using App.Api.Features.Projects.GetProjectDetails;
using App.Api.Features.Projects.ListProjectLookups;
using App.Api.Features.Projects.ListProjects;

namespace App.Api.Features.Projects;

public static class ProjectModule
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var projects = app.MapGroup("/projects")
            .WithTags("Projects");
        
        projects.MapGetProjectDetailsEndpoint();
        projects.MapListProjectLookupsEndpoint();
        projects.MapListProjectsEndpoint();
        projects.MapRestorePositionEndpoint();
    }
}