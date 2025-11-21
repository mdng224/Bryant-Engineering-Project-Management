using FluentValidation;

namespace App.Application.Positions.Commands.AddPosition;

public sealed class AddPositionCommandValidator : AbstractValidator<AddPositionCommand>
{
    public AddPositionCommandValidator()
    {
        RuleFor(apr => apr.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(128).WithMessage("Name must not exceed 128 characters.");

        RuleFor(apr => apr.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(16).WithMessage("Code must not exceed 16 characters.");
    }
}