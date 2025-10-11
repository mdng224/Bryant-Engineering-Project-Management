using FluentValidation;

namespace App.Api.Contracts.Auth;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters.")  // matches DB column and standard safe max.
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(256).WithMessage("Password cannot exceed 256 characters.")   //caps the input to prevent DoS and bcrypt’s 72-byte cutoff issue.
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit."); ;
    }
}
