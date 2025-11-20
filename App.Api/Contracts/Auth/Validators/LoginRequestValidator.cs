using App.Api.Features.Auth.Login;
using FluentValidation;

namespace App.Api.Contracts.Auth.Validators;

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
