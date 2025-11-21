using App.Domain.Common;

namespace App.Api.Features.Employees.AddEmployee;

public sealed record AddEmployeeRequest(
    string FirstName,
    string LastName,
    string? PreferredName,
    Guid? UserId,
    EmploymentType? EmploymentType,
    SalaryType? SalaryType,
    DepartmentType? Department,
    DateTimeOffset? HireDate,
    string CompanyEmail, // Email required for add requests, nullable on domain because of seed data
    string? WorkLocation,
    string? Notes,
    string? Line1,
    string? Line2,
    string? City,
    string? State,
    string? PostalCode,
    Guid? RecommendedRoleId
);
