namespace App.Api.Contracts.Employees.Responses;

// Non detailed row
public sealed record EmployeeSummaryResponse(
    Guid Id,
    string LastName,
    string FirstName,
    string? PreferredName,
    IReadOnlyList<string> PositionNames,
    string? Department,              // enum as string
    string? EmploymentType,          // enum as string
    DateTimeOffset? HireDate
);