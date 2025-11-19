using App.Domain.Common;

namespace App.Application.Employees.Commands.AddEmployee;

public record AddEmployeeCommand(
    string FirstName,
    string LastName,
    string? PreferredName,
    Guid? UserId,
    EmploymentType? EmploymentType,
    SalaryType? SalaryType,
    DepartmentType? Department,
    DateTimeOffset? HireDate,
    string? CompanyEmail,
    string? WorkLocation,
    string? Notes,
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    Guid? RecommendedRoleId);