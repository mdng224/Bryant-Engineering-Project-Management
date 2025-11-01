using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.EmployeePositions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.EmployeePositions;

public static class EmployeePositionsModule
{
    public static IServiceCollection AddEmployeePositionsApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<
            IQueryHandler<GetPositionsForEmployeesQuery, Result<IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>>>>,
            GetPositionsForEmployeesHandler>();

        // Commands

        return services;
    }
}