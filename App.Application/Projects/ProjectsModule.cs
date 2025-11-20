using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Projects;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Projects.Commands.RestoreProject;
using App.Application.Projects.Queries.GetProjectDetails;
using App.Application.Projects.Queries.GetProjectLookups;
using App.Application.Projects.Queries.GetProjects;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Projects;

public static class ProjectsModule
{
    public static IServiceCollection AddProjectsApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetProjectLookupsQuery, Result<ProjectLookupsDto>>, GetProjectLookupsHandler>();
        services.AddScoped<IQueryHandler<GetProjectDetailsQuery, Result<ProjectDetailsDto>>, GetProjectDetailsHandler>();
        services.AddScoped<IQueryHandler<ListProjectsQuery, Result<PagedResult<ProjectRowDto>>>, ListProjectsHandler>();
        
        // Commands
        services.AddScoped<ICommandHandler<RestoreProjectCommand, Result<Unit>>, RestoreProjectHandler>();
        
        return services;
    }
}