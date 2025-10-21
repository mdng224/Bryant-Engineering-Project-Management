using App.Domain.Common;
using App.Domain.Employees;
using App.Infrastructure.Persistence.Seed.Configurations;

namespace App.Infrastructure.Persistence.Seed;

internal static class EmployeeSeedFactory
{
    private static Employee SeededEmployee(
        Guid id,
        string last,
        string legalFirst,
        string? nickname,
        DepartmentType? departmentType,
        IEnumerable<Guid>? positionIds,
        EmploymentType? employmentType,
        SalaryType? salaryType,
        string? companyEmail
    )
    {
        var employee = new Employee(id, legalFirst, last);
        
        employee.SetPreferredName(nickname);
        employee.SetDepartment(departmentType);
        
        if (positionIds is not null)
            foreach (var pid in positionIds)
                employee.AddPosition(pid);
        
        employee.SetEmployment(employmentType, salaryType);
        employee.SetCompanyEmail(companyEmail);

        return employee;
    }

public static IEnumerable<Employee> All =>
[
    SeededEmployee(
        id: EmployeeIds.JasonBaker,
        last: "Baker",
        legalFirst: "James",
        nickname: "Jason",
        departmentType: DepartmentType.Engineering,
        positionIds: [PositionIds.PrincipalInCharge, PositionIds.ProjectEngineer],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Salary,
        companyEmail: "jason@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.DougBrown,
        last: "Brown",
        legalFirst: "Douglas",
        nickname: "Doug",
        departmentType: DepartmentType.Drafting,
        positionIds: [PositionIds.SeniorDraftingTechnician],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: "doug.brown@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.JakeDavis,
        last: "Davis",
        legalFirst: "Jacob",
        nickname: "Jake",
        departmentType: DepartmentType.Engineering,
        positionIds: [PositionIds.EngineeringIntern],
        employmentType: EmploymentType.PartTime,
        salaryType: SalaryType.Hourly,
        companyEmail: null
    ),
    SeededEmployee(
        id: EmployeeIds.GregHamilton,
        last: "Hamilton",
        legalFirst: "David",
        nickname: "Greg",
        departmentType: DepartmentType.Engineering,
        positionIds: [PositionIds.ProjectEngineer],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Salary,
        companyEmail: "greg.hamilton@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.BillKelly,
        last: "Kelly",
        legalFirst: "William",
        nickname: "Bill",
        departmentType: DepartmentType.Drafting,
        positionIds: [PositionIds.SeniorDraftingTechnician],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: "bill.kelly@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.MattLee,
        last: "Lee",
        legalFirst: "James",
        nickname: "Matt",
        departmentType: DepartmentType.Surveying,
        positionIds: [PositionIds.SurveyPartyChief],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: "matt.lee@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.AnthonyMayfield,
        last: "Mayfield",
        legalFirst: "Anthony",
        nickname: "Anthony",
        departmentType: DepartmentType.Surveying,
        positionIds: [PositionIds.Rodman],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: null
    ),
    SeededEmployee(
        id: EmployeeIds.VincentMayfield,
        last: "Mayfield",
        legalFirst: "Vincent",
        nickname: "Vincent",
        departmentType: DepartmentType.Surveying,
        positionIds: [PositionIds.Rodman],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: null
    ),
    SeededEmployee(
        id: EmployeeIds.NikkiMaynard,
        last: "Maynard",
        legalFirst: "Theresa",
        nickname: "Nikki",
        departmentType: DepartmentType.OfficeAdmin,
        positionIds: [PositionIds.OfficeManager],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: "nikki.maynard@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.TannerMiller,
        last: "Miller",
        legalFirst: "Jonathan",
        nickname: "Tanner",
        departmentType: DepartmentType.Drafting,
        positionIds: [PositionIds.DraftingTechnician],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: "tanner.miller@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.LeeMills,
        last: "Mills",
        legalFirst: "Anthony",
        nickname: "Lee",
        departmentType: DepartmentType.Engineering,
        positionIds: [PositionIds.PrincipalInCharge, PositionIds.ProjectEngineer],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Salary,
        companyEmail: "lee.mills@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.HtetThiriNaing,
        last: "Naing",
        legalFirst: "Htet Thiri",
        nickname: "Rose",
        departmentType: DepartmentType.Engineering,
        positionIds: [PositionIds.EngineeringTechnician],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: "htetthiri.naing@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.JacobOBryan,
        last: "O'Bryan",
        legalFirst: "Jacob",
        nickname: "Jacob",
        departmentType: DepartmentType.Drafting,
        positionIds: [PositionIds.DraftingTechnician],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: "jacob.obryan@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.TonyPhelps,
        last: "Phelps",
        legalFirst: "Mark",
        nickname: "Tony",
        departmentType: DepartmentType.Surveying,
        positionIds: [PositionIds.ProfessionalLandSurveyor],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Salary,
        companyEmail: "tony.phelps@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.MasonReynolds,
        last: "Reynolds",
        legalFirst: "Mason",
        nickname: "Mason",
        departmentType: DepartmentType.Surveying,
        positionIds: [PositionIds.SurveyPartyChief],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Hourly,
        companyEmail: "mason.reynolds@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.ChristianScrivner,
        last: "Scrivner",
        legalFirst: "James",
        nickname: "Christian",
        departmentType: DepartmentType.Engineering,
        positionIds: [PositionIds.EngineerInTraining],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Salary,
        companyEmail: "christian.scrivner@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.DavidWeaver,
        last: "Weaver",
        legalFirst: "Robert",
        nickname: "David",
        departmentType: DepartmentType.Engineering,
        positionIds: [PositionIds.PrincipalInCharge],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Salary,
        companyEmail: "david.weaver@bryant-eng.com"
    ),
    SeededEmployee(
        id: EmployeeIds.AndyWeaver,
        last: "Weaver",
        legalFirst: "William",
        nickname: "Andy",
        departmentType: DepartmentType.Engineering,
        positionIds: [PositionIds.PrincipalInCharge],
        employmentType: EmploymentType.FullTime,
        salaryType: SalaryType.Salary,
        companyEmail: "andy.weaver@bryant-eng.com"
    )
];
}
