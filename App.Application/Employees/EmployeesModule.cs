using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Employees.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Employees;

public static class EmployeesModule
{
    public static IServiceCollection AddEmployeesApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetEmployeesQuery, Result<GetEmployeesResult>>, GetEmployeesHandler>();

        // Commands

        return services;
    }
}