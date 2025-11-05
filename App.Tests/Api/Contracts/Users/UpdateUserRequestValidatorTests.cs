using App.Api.Contracts.Users;
using App.Api.Contracts.Users.Requests;
using App.Api.Contracts.Users.Validators;
using App.Domain.Users;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace App.Tests.Api.Contracts.Users;

public class UpdateUserRequestValidatorTests
{
    private readonly UpdateUserRequestValidator _validator = new();

    [Fact]
    public void Requires_At_Least_One_Field()
    {
        // Arrange
        var model = new UpdateUserRequest(RoleName: null, Status: null);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.Errors
            .Should()
            .ContainSingle(e => e.ErrorMessage == "Provide roleName and/or status.");
    }

    [Fact]
    public void Accepts_Only_Status_Change()
    {
        // Arrange
        var model = new UpdateUserRequest(RoleName: null, Status: UserStatus.Active);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Rejects_Whitespace_RoleName()
    {
        // Arrange
        var model = new UpdateUserRequest(RoleName: "   ", Status: null);

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
        var model = new UpdateUserRequest(RoleName: new string('A', 65), Status: null);

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
        var model = new UpdateUserRequest(RoleName: bad, Status: null);

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
        var model = new UpdateUserRequest(RoleName: ok, Status: null);

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(m => m.RoleName);
    }
}
