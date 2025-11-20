namespace App.Application.Common.Dtos.Employees;

public record EmployeeRowDto(
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