using App.Application.Auth;
using App.Application.EmployeePositions;
using App.Application.Employees;
using App.Application.Positions;
using App.Application.Users;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Modules
        services.AddAuthApplication();
        services.AddEmployeesApplication();
        services.AddPositionsApplication();
        services.AddUsersApplication();
        services.AddEmployeePositionsApplication();

        return services;
    }
}