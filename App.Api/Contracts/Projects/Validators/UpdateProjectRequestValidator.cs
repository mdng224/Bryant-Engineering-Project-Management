using App.Api.Contracts.Positions.Requests;
using FluentValidation;

namespace App.Api.Contracts.Projects.Validators;

public sealed class UpdateProjectRequestValidator : AbstractValidator<UpdateProjectRequest>
{
    public UpdateProjectRequestValidator()
    {
        RuleFor(upr => upr.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters.");
    }
}