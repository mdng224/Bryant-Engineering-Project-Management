using App.Domain.Common;

namespace App.Infrastructure.Persistence.Seed.Mappers;

internal static class EmployeeMapper
{
    public static DepartmentType? ToDepartmentType(this string? dt) => dt?.Trim() switch
    {
        "Engineering"  => DepartmentType.Engineering,
        "Drafting"     => DepartmentType.Drafting,
        "Surveying"    => DepartmentType.Surveying,
        "Office/Admin" => DepartmentType.OfficeAdmin,
        _              => null
    };
    
    public static EmploymentType? ToEmploymentType(this string? s) => s?.Trim() switch
    {
        "Full Time"    => EmploymentType.FullTime,
        "Part Time"    => EmploymentType.PartTime,
        _              => null
    };

    public static SalaryType? ToSalaryType(this string? s) => s?.Trim() switch
    {
        "Salary"       => SalaryType.Salary,
        "Hourly"       => SalaryType.Hourly,
        _              => null
    };
}