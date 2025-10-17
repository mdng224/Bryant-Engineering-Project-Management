using FluentValidation;

namespace App.Api.Contracts.Auth;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(lr => lr.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(lr => lr.Password)
            .NotEmpty();
    }
}
