namespace App.Api.Contracts.Employees;

// For details page
public sealed record EmployeeResponse(
    Guid Id,
    Guid? UserId,
    string FirstName,
    string LastName,
    string? PreferredName,
    string? EmploymentType,
    string? SalaryType,
    DateTimeOffset? HireDate,
    DateTimeOffset? EndDate,
    string? Department,
    string? CompanyEmail,
    string? WorkLocation,
    string? LicenseNotes,
    string? Notes,
    Guid? RecommendedRoleId,
    bool IsPreapproved,
    IReadOnlyList<string> PositionNames,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    bool IsActive
);