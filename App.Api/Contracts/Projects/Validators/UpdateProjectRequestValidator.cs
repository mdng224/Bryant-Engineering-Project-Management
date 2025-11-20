using App.Api.Features.Positions.UpdatePosition;
using FluentValidation;

namespace App.Api.Contracts.Projects.Validators;

public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdatePositionRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(upr => upr.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters.");
    }
}