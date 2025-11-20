using App.Api.Features.Positions.AddPosition;
using FluentValidation;

namespace App.Api.Contracts.Projects.Validators;

public sealed class AddPositionRequestValidator : AbstractValidator<AddPositionRequest>
{
    public AddPositionRequestValidator()
    {
        RuleFor(apr => apr.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters.");
    }
}