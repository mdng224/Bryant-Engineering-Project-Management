using App.Api.Contracts.Auth;
using App.Api.Contracts.Auth.Requests;
using App.Api.Contracts.Auth.Validators;
using FluentValidation.TestHelper;

namespace App.Tests.Api.Contracts.Auth;
public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public void Valid_Payload_Passes()
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", "SuperSecret1");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Email_Fails_With_Message()
    {
        // Arrange
        var request = new RegisterRequest(string.Empty, "supersecret1");

        // Act
        var result = _validator.TestValidate(request);

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
        var request = new RegisterRequest(badEmail, "SuperSecret1");

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email)
            .WithErrorMessage("A valid email address is required.");
    }

    [Fact]
    public void Empty_Password_Fails_With_Message()
    {
        // Arrange
        var request = new RegisterRequest("user@example.com", string.Empty);

        // Act
        var result = _validator.TestValidate(request);

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
        var request = new RegisterRequest("user@example.com", shortPassword);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must be at least 8 characters long.");
    }

    [Fact]
    public void Missing_Uppercase_Fails_With_Message()
    {
        // arrange
        var request = new RegisterRequest("user@example.com", "supersecret1"); // no uppercase

        // act
        var result = _validator.TestValidate(request);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one uppercase letter.");
    }

    [Fact]
    public void Missing_Digit_Fails_With_Message()
    {
        // arrange
        var request = new RegisterRequest("user@example.com", "SuperSecret"); // no digit

        // act
        var result = _validator.TestValidate(request);

        // assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password must contain at least one digit.");
    }

    [Fact]
    public void Boundary_Length_8_Passes_When_Other_Rules_Met()
    {
        // arrange
        var request = new RegisterRequest("user@example.com", "Abcdef1G"); // 8 chars

        // act
        var result = _validator.TestValidate(request);

        // assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Nulls_Fail_Both_Fields()
    {
        // Arrange
        var request = new RegisterRequest(null!, null!);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Email);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Length_72_Passes()
    {
        // Arrange
        var password = new string('A', 71) + "1"; // 72 chars incl. digit+uppercase

        // Act
        var result = _validator.TestValidate(new RegisterRequest("user@example.com", password));

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Password_Length_73_Fails()
    {
        // Arrange
        var password = new string('A', 72) + "1"; // 73

        // Act
        var result = _validator.TestValidate(new RegisterRequest("user@example.com", password));

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Password)
            .WithErrorMessage("Password cannot exceed 72 characters.");
    }

    [Fact]
    public void Email_Max_254_Passes()
    {
        // Arrange
        var local = new string('a', 64);
        var domain = string.Join(".", new string('b', 63), new string('c', 63), "d".PadLeft(61, 'd')); // craft to reach 254 total
        var email = $"{local}@{domain}"[..254]; // ensure 254

        // Act
        var result = _validator.TestValidate(new RegisterRequest(email, "SuperSecret1"));

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

}
