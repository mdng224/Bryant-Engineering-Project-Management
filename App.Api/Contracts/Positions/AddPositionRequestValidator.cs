using FluentValidation;

namespace App.Api.Contracts.Positions;

public sealed class AddPositionRequestValidator : AbstractValidator<AddPositionRequest>
{
    public AddPositionRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(20).WithMessage("Code must not exceed 20 characters.");

        RuleFor(x => x.RequiresLicense)
            .NotNull().WithMessage("RequiresLicense must be specified.");
    }
}