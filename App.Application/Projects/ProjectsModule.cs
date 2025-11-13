using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Projects.Commands.RestoreProject;
using App.Application.Projects.Queries.GetProjects;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Projects;

public static class ProjectsModule
{
    public static IServiceCollection AddProjectsApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetProjectsQuery, Result<PagedResult<ProjectListItemDto>>>, GetProjectsHandler>();

        // Commands
        services.AddScoped<ICommandHandler<RestoreProjectCommand, Result<Unit>>, RestoreProjectHandler>();
        
        return services;
    }
}