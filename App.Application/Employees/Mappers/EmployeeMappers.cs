using App.Application.Employees.Commands.AddEmployee;
using App.Domain.Employees;

namespace App.Application.Employees.Mappers;

public static class EmployeeMappers
{
    public static Employee ToDomain(this AddEmployeeCommand command) =>
        new(
            firstName:       command.FirstName,
            lastName:        command.LastName,
            preferredName:   command.PreferredName,
            userId:          command.UserId,
            employmentType:  command.EmploymentType,
            salaryType:      command.SalaryType,
            department:      command.Department,
            hireDate:        command.HireDate,
            companyEmail:    command.CompanyEmail,
            workLocation:    command.WorkLocation,
            notes:           command.Notes,
            addressLine1:    command.AddressLine1,
            addressLine2:    command.AddressLine2,
            city:            command.City,
            state:           command.State,
            postalCode:      command.PostalCode,
            recommendedRoleId: command.RecommendedRoleId,
            isPreapproved:   true
        );
}