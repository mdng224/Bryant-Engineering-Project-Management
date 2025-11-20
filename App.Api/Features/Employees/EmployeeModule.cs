using App.Api.Features.Employees.AddEmployee;
using App.Api.Features.Employees.GetEmployeeDetails;
using App.Api.Features.Employees.ListEmployees;
using App.Api.Features.Employees.RestoreEmployee;

namespace App.Api.Features.Employees;

public static class EmployeeModule
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var employees = app.MapGroup("/employees")
            .WithTags("Employees");

        employees.MapAddEmployeeEndpoint();
        employees.MapGetEmployeeDetailsEndpoint();
        employees.MapListEmployeesEndpoint();
        employees.MapRestoreEmployeeEndpoint();
    }
}