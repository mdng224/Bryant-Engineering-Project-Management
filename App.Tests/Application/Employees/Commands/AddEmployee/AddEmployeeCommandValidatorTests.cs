using App.Application.Employees.Commands.AddEmployee;
using App.Domain.Common;
using FluentValidation.TestHelper;

namespace App.Tests.Application.Employees.Commands.AddEmployee;

public class AddEmployeeCommandValidatorTests
{
    private readonly AddEmployeeCommandValidator _validator = new();

    private static AddEmployeeCommand CreateValidCommand() =>
        new(
            FirstName:      "John",
            LastName:       "Doe",
            PreferredName:  "Johnny",
            UserId:         null,
            EmploymentType: EmploymentType.FullTime,
            SalaryType:     SalaryType.Salary,
            Department:     DepartmentType.Engineering,
            HireDate:       null,
            CompanyEmail:   "john.doe@example.com",
            WorkLocation:   "Denver Office",
            Line1:          "123 Main St",
            Line2:          "Suite 100",
            City:           "Denver",
            State:          "CO",
            PostalCode:     "80014"
        );

    [Fact]
    public void ValidCommand_Should_PassValidation()
    {
        var cmd = CreateValidCommand();

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void FirstName_Empty_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { FirstName = string.Empty };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.FirstName);
    }

    [Fact]
    public void FirstName_TooLong_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { FirstName = new string('a', 101) };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.FirstName);
    }

    [Fact]
    public void LastName_Empty_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { LastName = string.Empty };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.LastName);
    }

    [Fact]
    public void LastName_TooLong_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { LastName = new string('a', 101) };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.LastName);
    }

    [Fact]
    public void PreferredName_Null_Should_PassValidation()
    {
        var cmd = CreateValidCommand() with { PreferredName = null };

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(aec => aec.PreferredName);
    }

    [Fact]
    public void PreferredName_Empty_Should_PassValidation()
    {
        var cmd = CreateValidCommand() with { PreferredName = string.Empty };

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(aec => aec.PreferredName);
    }

    [Fact]
    public void PreferredName_TooLong_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { PreferredName = new string('a', 101) };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.PreferredName);
    }

    [Fact]
    public void CompanyEmail_Empty_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { CompanyEmail = string.Empty };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.CompanyEmail);
    }

    [Fact]
    public void CompanyEmail_InvalidFormat_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { CompanyEmail = "not-an-email" };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.CompanyEmail);
    }

    [Fact]
    public void CompanyEmail_Valid_Should_PassValidation()
    {
        var cmd = CreateValidCommand() with { CompanyEmail = "valid.email@example.com" };

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(aec => aec.CompanyEmail);
    }

    [Fact]
    public void WorkLocation_Null_Should_PassValidation()
    {
        var cmd = CreateValidCommand() with { WorkLocation = null };

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(aec => aec.WorkLocation);
    }

    [Fact]
    public void WorkLocation_Empty_Should_PassValidation()
    {
        var cmd = CreateValidCommand() with { WorkLocation = string.Empty };

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(aec => aec.WorkLocation);
    }

    [Fact]
    public void WorkLocation_TooLong_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { WorkLocation = new string('a', 201) };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.WorkLocation);
    }

    [Fact]
    public void EmploymentType_InvalidEnum_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { EmploymentType = (EmploymentType)999 };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.EmploymentType);
    }

    [Fact]
    public void SalaryType_InvalidEnum_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { SalaryType = (SalaryType)999 };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.SalaryType);
    }

    [Fact]
    public void Department_InvalidEnum_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { Department = (DepartmentType)999 };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec.Department);
    }

    [Fact]
    public void Address_AllEmpty_Should_PassValidation()
    {
        var cmd = CreateValidCommand() with
        {
            Line1      = null,
            Line2      = null,
            City       = null,
            State      = null,
            PostalCode = null
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(aec => aec);
    }

    [Fact]
    public void Address_AllRequiredFieldsPresent_Should_PassValidation()
    {
        var cmd = CreateValidCommand();

        var result = _validator.TestValidate(cmd);

        result.ShouldNotHaveValidationErrorFor(aec => aec);
    }

    [Fact]
    public void Address_Partial_MissingCity_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { City = null };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec);
    }

    [Fact]
    public void Address_Partial_MissingState_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { State = null };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec);
    }

    [Fact]
    public void Address_Partial_MissingPostalCode_Should_HaveError()
    {
        var cmd = CreateValidCommand() with { PostalCode = null };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec);
    }

    [Fact]
    public void Address_Partial_OnlyLine1_Should_HaveError()
    {
        var cmd = CreateValidCommand() with
        {
            City       = null,
            State      = null,
            PostalCode = null
        };

        var result = _validator.TestValidate(cmd);

        result.ShouldHaveValidationErrorFor(aec => aec);
    }
}
