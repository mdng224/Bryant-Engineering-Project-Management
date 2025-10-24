namespace App.Api.Contracts.Employees;

// One row in the table
public sealed record EmployeeListItem(
    EmployeeSummaryResponse Summary,
    EmployeeResponse Details
);