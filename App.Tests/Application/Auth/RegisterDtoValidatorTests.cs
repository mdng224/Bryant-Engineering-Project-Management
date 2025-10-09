using App.Application.Auth;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace App.Tests.Application.Auth;

public class RegisterDtoValidatorTests
{
    [Fact]
    public void Should_Fail_When_Password_Too_Weak()
    {
        // Arrange
        var v = new RegisterDtoValidator();

        // Act
        var r = v.TestValidate(new RegisterDto("me@example.com", "weakpass"));

        // Assert
        r.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_Pass_For_Strong_Password()
    {
        // Arrange
        var v = new RegisterDtoValidator();
        var dto = new RegisterDto("me@example.com", "Strong1Pass");

        // Act
        var r = v.TestValidate(dto);

        // Assert
        r.IsValid.Should().BeTrue();
    }
}
