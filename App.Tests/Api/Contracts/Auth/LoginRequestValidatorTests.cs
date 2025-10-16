using App.Api.Contracts.Auth;
using FluentValidation.TestHelper;

namespace App.Tests.Api.Contracts.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Valid_Payload_Passes()
    {
        // Arrange
        var loginRequest = new LoginRequest("user@example.com", "secret");

        // Act
        var result = _validator.TestValidate(loginRequest);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_Fails()
    {
        // Arrange
        var loginRequest = new LoginRequest(string.Empty, "secret");

        // Act
        var result = _validator.TestValidate(loginRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("missing-at.example.com")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    public void Invalid_Email_Format_Fails(string badEmail)
    {
        // Arrange
        var loginRequest = new LoginRequest(badEmail, "secret");

        // Act
        var result = _validator.TestValidate(loginRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Empty_Password_Fails()
    {
        // Arrange
        var loginRequest = new LoginRequest("user@example.com", string.Empty);

        // Act
        var result = _validator.TestValidate(loginRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Nulls_Fail_For_Both_Fields()
    {
        // Arrange
        var loginRequest = new LoginRequest(null!, null!); // record allows null compile-time with !

        // Act
        var result = _validator.TestValidate(loginRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
