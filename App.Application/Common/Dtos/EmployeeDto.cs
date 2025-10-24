namespace App.Application.Common.Dtos;

public sealed record EmployeeDto(
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
    IReadOnlyList<Guid> PositionIds,
    // Contact / Misc
    string? CompanyEmail,
    string? WorkLocation,
    string? LicenseNotes,
    string? Notes,
    Guid? RecommendedRoleId,
    bool IsPreapproved,
    // Auditing
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    // Convenience
    bool IsActive
);