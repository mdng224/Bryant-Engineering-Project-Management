using FluentValidation;
using System.Text.RegularExpressions;

namespace App.Api.Contracts.Admins;

public sealed class PatchUserRequestValidator : AbstractValidator<PatchUserRequest>
{
    private static readonly Regex RoleRegex = new("^[A-Za-z0-9 _-]+$", RegexOptions.Compiled);

    public PatchUserRequestValidator()
    {
        // Require at least one field
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.RoleName) || x.IsActive.HasValue)
            .WithMessage("Provide roleName and/or isActive.");

        // Validate role only when provided
        When(x => x.RoleName is not null, () =>
        {
            RuleFor(x => x.RoleName!)
                .Cascade(CascadeMode.Stop)
                .Must(s => !string.IsNullOrWhiteSpace(s)).WithMessage("roleName cannot be empty.")
                .Must(s => s!.Trim().Length <= 64).WithMessage("roleName is too long.")
                .Must(s => RoleRegex.IsMatch(s!.Trim())).WithMessage("roleName contains invalid characters.");
        });
    }
}