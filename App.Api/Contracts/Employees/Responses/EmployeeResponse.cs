namespace App.Api.Contracts.Employees.Responses;

// For details page
public sealed record EmployeeResponse(
    Guid Id,
    Guid? UserId,
    string FirstName,
    string LastName,
    string? PreferredName,
    IReadOnlyList<string> PositionNames,
    string? EmploymentType,
    string? SalaryType,
    DateTimeOffset? HireDate,
    DateTimeOffset? EndDate,
    string? Department,
    string? CompanyEmail,
    string? WorkLocation,
    string? Notes,
    Guid? RecommendedRoleId,
    bool IsPreapproved,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc
);