using FluentValidation;

namespace App.Application.Employees.Commands.AddEmployee;

public sealed class AddEmployeeCommandValidator : AbstractValidator<AddEmployeeCommand>
{
    public AddEmployeeCommandValidator()
    {
        RuleFor(c => c.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(c => c.LastName)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(c => c.CompanyEmail)
            .NotEmpty().WithMessage("Company email is required.")
            .EmailAddress().WithMessage("Company email must be a valid email.");
        
        RuleFor(c => c.CompanyEmail)
            .EmailAddress()
            .When(c => !string.IsNullOrWhiteSpace(c.CompanyEmail));

        RuleFor(c => c.SalaryType)
            .IsInEnum()
            .When(c => c.SalaryType.HasValue);

        RuleFor(c => c.EmploymentType)
            .IsInEnum()
            .When(c => c.EmploymentType.HasValue);

        RuleFor(c => c.Department)
            .IsInEnum()
            .When(c => c.Department.HasValue);

        // address all-or-nothing rule
        RuleFor(c => c)
            .Must(AllOrNothingAddress)
            .WithMessage("Address must include line1, city, state, and postalCode, or be omitted entirely.");
    }

    private static bool AllOrNothingAddress(AddEmployeeCommand cmd)
    {
        var hasAny = !string.IsNullOrWhiteSpace(cmd.Line1)
                     || !string.IsNullOrWhiteSpace(cmd.City)
                     || !string.IsNullOrWhiteSpace(cmd.State)
                     || !string.IsNullOrWhiteSpace(cmd.PostalCode);

        if (!hasAny) return true; // all empty → valid

        return !string.IsNullOrWhiteSpace(cmd.Line1)
               && !string.IsNullOrWhiteSpace(cmd.City)
               && !string.IsNullOrWhiteSpace(cmd.State)
               && !string.IsNullOrWhiteSpace(cmd.PostalCode);
    }
}