using FluentValidation;

namespace App.Api.Contracts.Positions;

public sealed class AddPositionRequestValidator : AbstractValidator<AddPositionRequest>
{
    public AddPositionRequestValidator()
    {
        RuleFor(apr => apr.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters.");

        RuleFor(apr => apr.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(16).WithMessage("Code must not exceed 16 characters.");
    }
}