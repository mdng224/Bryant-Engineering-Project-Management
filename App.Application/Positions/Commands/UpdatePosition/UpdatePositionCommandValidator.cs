using FluentValidation;

namespace App.Application.Positions.Commands.UpdatePosition;

public sealed class UpdatePositionCommandValidator : AbstractValidator<UpdatePositionCommand>
{
    public UpdatePositionCommandValidator()
    {
        RuleFor(upr => upr.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters.");

        RuleFor(upr => upr.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(16).WithMessage("Code must not exceed 16 characters.");
    }
}