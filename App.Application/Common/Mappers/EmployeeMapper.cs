using App.Application.Common.Dtos;
using App.Domain.Employees;

namespace App.Application.Common.Mappers;

public static class EmployeeMapper
{
    private static EmployeeDto ToDto(this Employee e) =>
        new EmployeeDto(
            e.Id,
            e.UserId,
            e.FirstName,
            e.LastName,
            e.PreferredName,
            e.EmploymentType == null
                ? null
                : e.EmploymentType.ToString(),
            e.SalaryType == null
                ? null
                : e.SalaryType.ToString(),
            e.HireDate,
            e.EndDate,
            e.Department == null
                ? null
                : e.Department.ToString(),
            e.Positions.Select(p => p.PositionId)
                .ToList(), // IReadOnlyList<Guid>
            e.CompanyEmail,
            e.WorkLocation,
            e.Notes,
            e.RecommendedRoleId,
            e.IsPreapproved,
            e.CreatedAtUtc,
            e.UpdatedAtUtc,
            e.DeletedAtUtc,
            // Convenience flag computed in projection
            e.DeletedAtUtc == null && e.EndDate == null
        );

    public static IEnumerable<EmployeeDto> ToDto(this IEnumerable<Employee> employees) => employees.Select(ToDto);
}