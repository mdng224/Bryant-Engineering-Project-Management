using App.Domain.Employees;
using App.Domain.Security;
using App.Infrastructure.Persistence.Seed;
using App.Infrastructure.Persistence.Seed.Configurations;
using FluentAssertions;

namespace App.Tests.Infrastructure.Persistence.Seed;

public class EmployeeSeedFactoryTests
{
    [Fact]
    public void Preapprove_without_email_throws()
    {
        // Arrange
        var employee = new Employee("A", "B");

        // Act
        var action = () => employee.SetPreapproved(true);

        // Assert
        action.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("*without a company email*");
    }

    [Fact]
    public void Clearing_company_email_disables_preapproval()
    {
        // Arrange
        var employee = new Employee("A", "B");
        employee.SetCompanyEmail("a@x.com");
        employee.SetPreapproved(true);

        // Act
        employee.SetCompanyEmail(null);

        // Assert
        employee.IsPreapproved.Should().BeFalse();
    }

    [Fact]
    public void AddPosition_is_idempotent()
    {
        // Arrange
        var employee = new Employee("A", "B");
        var positionId = Guid.NewGuid();

        // Act
        var firstAdd = employee.AddPosition(positionId);
        var secondAdd = employee.AddPosition(positionId);

        // Assert
        ReferenceEquals(firstAdd, secondAdd)
            .Should()
            .BeTrue("adding the same position twice should return the same object");
        employee.Positions.Should().HaveCount(1);
    }

    [Fact]
    public void Seed_andy_is_admin_and_preapproved_with_email()
    {
        // Arrange
        var employee = EmployeeSeedFactory.All.Single(x => x.Id == EmployeeIds.AndyWeaver);

        // Assert
        employee.CompanyEmail.Should().Be("andy.weaver@bryant-eng.com");
        employee.IsPreapproved.Should().BeTrue();
        employee.RecommendedRoleId.Should().Be(RoleIds.Administrator);
    }

    [Fact]
    public void Seed_users_without_email_are_not_preapproved()
    {
        // Arrange
        var noEmailEmployees = EmployeeSeedFactory.All.Where(x => x.CompanyEmail is null);

        // Assert
        noEmailEmployees.Should().OnlyContain(x => x.IsPreapproved == false);
    }
}
