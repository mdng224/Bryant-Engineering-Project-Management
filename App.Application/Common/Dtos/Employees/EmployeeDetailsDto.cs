namespace App.Application.Common.Dtos.Employees;

public record EmployeeDetailsDto(
    Guid Id,
    Guid? UserId,
    // Identity
    string FirstName,
    string LastName,
    string? PreferredName,
    // Employment
    string? EmploymentType,   // "FullTime" | "PartTime"
    string? SalaryType,       // "Salary" | "Hourly"
    DateTimeOffset? HireDate,
    DateTimeOffset? EndDate,
    // Organization
    string? Department,
    IReadOnlyList<string> PositionNames,
    // Contact / Misc
    string? CompanyEmail,
    string? WorkLocation,
    string? Notes,
    Guid? RecommendedRoleId,
    bool IsPreapproved,
    // Auditing
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc
);