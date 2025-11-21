using FluentValidation;

namespace App.Application.Auth.Queries.Login;

public sealed class LoginQueryValidator : AbstractValidator<LoginQuery>
{
    public LoginQueryValidator()
    {
        RuleFor(lr => lr.Email)
            .NotEmpty()
            .EmailAddress();
        RuleFor(lr => lr.Password)
            .NotEmpty();
    }
}
