using App.Application.Abstractions.Handlers;
using App.Application.Clients.Commands.AddClient;
using App.Application.Common.Dtos;
using App.Application.Common.Dtos.Employees;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Employees.Commands.AddEmployee;
using App.Application.Employees.Commands.RestoreEmployee;
using App.Application.Employees.Queries;
using App.Application.Employees.Queries.GetEmployeeDetails;
using App.Application.Positions.Commands.RestorePosition;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Employees;

public static class EmployeesModule
{
    public static IServiceCollection AddEmployeesApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetEmployeeDetailsQuery, Result<EmployeeDetailsDto>>, GetEmployeeDetailsHandler>();
        services.AddScoped<IQueryHandler<ListEmployeesQuery, Result<PagedResult<EmployeeRowDto>>>, GetEmployeesHandler>();
        
        // Commands
        services.AddScoped<ICommandHandler<AddEmployeeCommand, Result<Guid>>, AddEmployeeHandler>();
        services.AddScoped<ICommandHandler<RestoreEmployeeCommand, Result<Unit>>, RestoreEmployeeHandler>();
        
        return services;
    }
}