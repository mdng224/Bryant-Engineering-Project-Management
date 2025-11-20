using App.Domain.Common;

namespace App.Api.Features.Employees.GetEmployeeDetails;


public sealed record GetEmployeeDetailsResponse(
    Guid Id,
    Guid? UserId,
    string FirstName,
    string LastName,
    string? PreferredName,
    string FullName,
    // Employment
    string? EmploymentType,
    string? SalaryType,
    string? Department,
    DateTimeOffset? HireDate,
    DateTimeOffset? EndDate,
    // Contact / Work
    string? CompanyEmail,
    string? WorkLocation,
    string? Notes,
    // Positions
    IReadOnlyList<string> PositionNames,
    // Address (flattened from Address VO)
    string? AddressLine1,
    string? AddressLine2,
    string? City,
    string? State,
    string? PostalCode,
    // Role recommendation / preapproval
    string? RecommendedRole,
    bool IsPreapproved,
    // Audit
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? DeletedAtUtc,
    Guid? CreatedById,
    Guid? UpdatedById,
    string? DeletedBy
);