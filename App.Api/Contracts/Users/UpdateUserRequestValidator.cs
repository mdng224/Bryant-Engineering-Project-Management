using System.Text.RegularExpressions;
using FluentValidation;

namespace App.Api.Contracts.Users;

public sealed class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    private static readonly Regex RoleRegex = new("^[A-Za-z0-9 _-]+$", RegexOptions.Compiled);

    public UpdateUserRequestValidator()
    {
        // Require at least one field
        RuleFor(uur => uur)
            .Must(uur => !string.IsNullOrWhiteSpace(uur.RoleName) || uur.Status.HasValue)
            .WithMessage("Provide roleName and/or status.");

        // Validate role only when provided
        When(x => x.RoleName is not null, () =>
        {
            RuleFor(uur => uur.RoleName!)
                .Cascade(CascadeMode.Stop)
                .Must(s => !string.IsNullOrWhiteSpace(s))
                    .WithMessage("roleName cannot be empty.")
                .Must(s => s!.Trim().Length <= 64)
                    .WithMessage("roleName is too long.")
                .Must(s => RoleRegex.IsMatch(s!.Trim()))
                    .WithMessage("roleName contains invalid characters.");
        });
        
        // Validate status only when provided
        When(uur => uur.Status.HasValue, () =>
        {
            // If Status is an enum (UserStatus), this ensures it's a defined value
            RuleFor(uur => uur.Status!.Value)
                .IsInEnum()
                .WithMessage("status is invalid.");
        });
    }
}