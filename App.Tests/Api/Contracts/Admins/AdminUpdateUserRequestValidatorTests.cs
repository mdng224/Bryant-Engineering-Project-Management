using App.Api.Contracts.Admins;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace App.Tests.Api.Contracts.Admins;

public class AdminUpdateUserRequestValidatorTests
{
    private readonly AdminUpdateUserRequestValidator _validator = new();

    [Fact]
    public void Requires_At_Least_One_Field()
    {
        // Arrange
        var model = new AdminUpdateUserRequest(RoleName: null, IsActive: null);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        // Top-level rule -> easiest is to check any error with the expected message
        result.Errors
            .Should()
            .ContainSingle(e => e.ErrorMessage == "Provide roleName and/or isActive.");
    }

    [Fact]
    public void Accepts_Only_IsActive_Change()
    {
        // Arrange
        var model = new AdminUpdateUserRequest(RoleName: null, IsActive: true);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Rejects_Whitespace_RoleName()
    {
        // Arrange
        var model = new AdminUpdateUserRequest(RoleName: "   ", IsActive: null);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.RoleName)
            .WithErrorMessage("roleName cannot be empty.");
    }

    [Fact]
    public void Rejects_RoleName_Too_Long()
    {
        // Arrange
        var model = new AdminUpdateUserRequest(RoleName: new string('A', 65), IsActive: null);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.RoleName)
            .WithErrorMessage("roleName is too long.");
    }

    [Theory]
    [InlineData("Admin!")]
    [InlineData("Admin@Root")]
    [InlineData("Admin#1")]
    [InlineData("Admin\tRoot")]
    public void Rejects_RoleName_With_Invalid_Characters(string bad)
    {
        // Arrange
        var model = new AdminUpdateUserRequest(RoleName: bad, IsActive: null);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(m => m.RoleName)
            .WithErrorMessage("roleName contains invalid characters.");
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Admin Root")]
    [InlineData("Admin-Root")]
    [InlineData("Admin_Root")]
    [InlineData("User123")]
    public void Accepts_RoleName_With_Allowed_Characters(string ok)
    {
        // Arrange
        var model = new AdminUpdateUserRequest(RoleName: ok, IsActive: null);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.RoleName);
    }
}