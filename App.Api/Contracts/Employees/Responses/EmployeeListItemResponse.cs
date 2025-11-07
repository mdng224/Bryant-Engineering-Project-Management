namespace App.Api.Contracts.Employees.Responses;

// One row in the table
public sealed record EmployeeListItemResponse(
    EmployeeSummaryResponse Summary,
    EmployeeResponse Details
);