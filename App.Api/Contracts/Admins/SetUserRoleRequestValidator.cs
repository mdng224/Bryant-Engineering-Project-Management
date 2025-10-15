using App.Domain.Security;
using FluentValidation;

namespace App.Api.Contracts.Admins;

public sealed class SetUserRoleRequestValidator : AbstractValidator<UpdateUserRequest>
{
    private static readonly HashSet<string> Allowed =
        new(RoleNames.All, StringComparer.OrdinalIgnoreCase);

    public SetUserRoleRequestValidator()
    {
        RuleFor(x => x.RoleName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("RoleName is required.")
            .Must(r => r == r!.Trim()).WithMessage("RoleName cannot start or end with spaces.")
            .Must(r => Allowed.Contains(r!))
                .WithMessage($"RoleName must be one of: {string.Join(", ", RoleNames.All)}")
            .MaximumLength(100);
    }
}