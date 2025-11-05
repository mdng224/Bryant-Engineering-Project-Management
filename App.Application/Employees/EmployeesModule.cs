using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Employees.Commands.RestoreEmployee;
using App.Application.Employees.Queries;
using App.Application.Positions.Commands.RestorePosition;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Employees;

public static class EmployeesModule
{
    public static IServiceCollection AddEmployeesApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetEmployeesQuery, Result<PagedResult<EmployeeDto>>>, GetEmployeesHandler>();

        // Commands
        services.AddScoped<ICommandHandler<RestoreEmployeeCommand, Result<EmployeeDto>>, RestoreEmployeeHandler>();
        
        return services;
    }
}