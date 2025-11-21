using App.Domain.Common;
using App.Domain.Employees;

namespace App.Tests.Domain.Employees;

public static class EmployeeTestFactory
{
    internal static Employee CreateActive(Guid id)
    {
        // use your real Employee ctor + any methods you have to ensure IsDeleted == false
        var employee = new Employee(
            firstName: "John",
            lastName: "Doe",
            preferredName: null,
            userId: null,
            employmentType: EmploymentType.FullTime,
            salaryType: SalaryType.Salary,
            department: DepartmentType.Engineering,
            hireDate: DateTimeOffset.UtcNow,
            companyEmail: "john.doe@example.com",
            workLocation: "Denver",
            notes: null,
            line1: null,
            line2: null,
            city: null,
            state: null,
            postalCode: null,
            recommendedRoleId: null,
            isPreapproved: false
        );

        // optionally set Id via reflection if you need it to match `id`
        return employee;
    }

    public static Employee CreateDeleted(Guid id)
    {
        var employee = CreateActive(id);
        employee.SoftDelete(); // or whatever method your domain exposes to soft-delete
        return employee;
    }
}