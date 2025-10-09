using App.Application.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace App.Tests.Application.Auth;

public class LoginDtoValidatorTests
{
    [Fact]
    public void Should_Fail_When_Email_Missing_Or_Invalid()
    {
        // Arrange
        var v = new LoginDtoValidator();

        // Act
        var r1 = v.TestValidate(new LoginDto("", "password123"));
        var r2 = v.TestValidate(new LoginDto("not-an-email", "password123"));

        // Assert
        r1.ShouldHaveValidationErrorFor(x => x.Email);
        r2.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Fail_When_Password_Short()
    {
        // Arrange
        var v = new LoginDtoValidator();

        // Act
        var r = v.TestValidate(new LoginDto("me@example.com", "short"));

        // Assert
        r.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Pass_For_Valid_Input()
    {
        // Arrange
        var v = new LoginDtoValidator();
        var dto = new LoginDto("me@example.com", "GoodPassw0rd");

        // Act
        var r = v.TestValidate(dto);

        // Assert
        r.IsValid.Should().BeTrue();
        r.Errors.Should().BeEmpty();
    }
}
