using App.Api.Contracts.Positions;
using App.Api.Contracts.Positions.Requests;
using App.Api.Contracts.Positions.Validators;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace App.Tests.Api.Contracts.Positions;

public class AddProjectRequestValidatorTests
{
    private readonly AddProjectRequestValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var model = new AddProjectRequest(Name: "", Code: "ENG", RequiresLicense: true);
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Pass_With_Valid_Data()
    {
        var model = new AddProjectRequest(Name: "Engineer", Code: "ENG", RequiresLicense: false);
        var result = _validator.TestValidate(model);
        result.IsValid.Should().BeTrue();
    }
}