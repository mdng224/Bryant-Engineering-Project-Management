using App.Api.Contracts.Auth;
using FluentValidation.TestHelper;

namespace App.Tests.Api.Contracts.Auth;
public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Valid_Payload_Passes()
    {
        // Arrange
        var registerRequest = new RegisterRequest("user@example.com", "SuperSecret1");

        // Act
        var result = _validator.TestValidate(registerRequest);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_Fails_With_Message()
    {
        // Arrange
        var registerRequest = new RegisterRequest(string.Empty, "supersecret1");

        // Act
        var result = _validator.TestValidate(registerRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("Email is required.");
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("missing-at.example.com")]
    [InlineData("user@")]
    [InlineData("@example.com")]
    public void Invalid_Email_Format_Fails(string badEmail)
    {
        // Arrange
        var registerRequest = new RegisterRequest(badEmail, "SuperSecret1");

        // Act
        var result = _validator.TestValidate(registerRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
              .WithErrorMessage("A valid email address is required.");
    }

    [Fact]
    public void Empty_Password_Fails_With_Message()
    {
        // Arrange
        var registerRequest = new RegisterRequest("user@example.com", string.Empty);

        // Act
        var result = _validator.TestValidate(registerRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password is required.");
    }

    [Theory]
    [InlineData("short")]     // < 8
    [InlineData("1234567")]   // 7 chars
    public void Short_Password_Fails_With_Message(string shortPassword)
    {
        // Arrange
        var registerRequest = new RegisterRequest("user@example.com", shortPassword);

        // Act
        var result = _validator.TestValidate(registerRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must be at least 8 characters long.");
    }

    [Fact]
    public void Missing_Uppercase_Fails_With_Message()
    {
        // arrange
        var registerRequest = new RegisterRequest("user@example.com", "supersecret1"); // no uppercase

        // act
        var result = _validator.TestValidate(registerRequest);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Missing_Digit_Fails_With_Message()
    {
        // arrange
        var registerRequest = new RegisterRequest("user@example.com", "SuperSecret"); // no digit

        // act
        var result = _validator.TestValidate(registerRequest);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
              .WithErrorMessage("Password must contain at least one digit.");
    }

    [Fact]
    public void Boundary_Length_8_Passes_When_Other_Rules_Met()
    {
        // arrange
        var registerRequest = new RegisterRequest("user@example.com", "Abcdef1G"); // 8 chars

        // act
        var result = _validator.TestValidate(registerRequest);

        // assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Nulls_Fail_Both_Fields()
    {
        // Arrange
        var registerRequest = new RegisterRequest(null!, null!);

        // Act
        var result = _validator.TestValidate(registerRequest);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
