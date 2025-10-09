using FluentValidation;

namespace App.Application.Auth;

public class RegisterDtoValidator : AbstractValidator<RegisterDto>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8)
            .Matches(@"[A-Z]").WithMessage("Password needs an uppercase letter.")
            .Matches(@"[a-z]").WithMessage("Password needs a lowercase letter.")
            .Matches(@"\d").WithMessage("Password needs a digit.");
    }
}