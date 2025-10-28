using App.Api.Contracts.Positions;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace App.Tests.Api.Contracts.Positions;

public class AddPositionRequestValidatorTests
{
    private readonly AddPositionRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = new AddPositionRequest(Name: "", Code: "ENG", RequiresLicense: true);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Pass_With_Valid_Data()
    {
        var model = new AddPositionRequest(Name: "Engineer", Code: "ENG", RequiresLicense: false);
        var result = _validator.TestValidate(model);
        result.IsValid.Should().BeTrue();
    }
}