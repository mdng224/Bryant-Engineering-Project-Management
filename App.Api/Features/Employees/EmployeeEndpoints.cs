using System.Reflection.Metadata;
using App.Api.Contracts.Employees;

namespace App.Api.Features.Employees;

public static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var employees = app.MapGroup("/employees")
            .WithTags("Employees");

        // GET /employees?page=&pageSize=
        employees.MapGet("/", GetEmployees.Handle)
            .WithSummary("List all employees (paginated)")
            .Produces<GetEmployeesResponse>();
    }
}