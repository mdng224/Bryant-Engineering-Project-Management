namespace App.Api.Features.Employees.ListEmployees;

public sealed record EmployeeRowResponse(
    Guid Id,
    string LastName,
    string FirstName,
    string? PreferredName,
    IReadOnlyList<string> PositionNames,
    string? Department,
    string? EmploymentType,
    DateTimeOffset? HireDate,
    DateTimeOffset? DeletedAtUtc
);