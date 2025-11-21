using FluentValidation;

namespace App.Application.Employees.Commands.AddEmployee;

public sealed class AddEmployeeCommandValidator : AbstractValidator<AddEmployeeCommand>
{
    public AddEmployeeCommandValidator()
    {
        RuleFor(aec => aec.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(aec => aec.LastName)
            .NotEmpty()
            .MaximumLength(100);
        
        RuleFor(aec => aec.PreferredName)
            .MaximumLength(100)
            .When(c => !string.IsNullOrWhiteSpace(c.PreferredName));

        
        RuleFor(aec => aec.CompanyEmail)
            .NotEmpty().WithMessage("Company email is required.")
            .EmailAddress().WithMessage("Company email must be a valid email.");

        RuleFor(aec => aec.WorkLocation)
            .MaximumLength(200)
            .When(c => !string.IsNullOrWhiteSpace(c.WorkLocation));
        
        RuleFor(aec => aec.EmploymentType)
            .NotNull().WithMessage("Employment type is required.")
            .IsInEnum();

        RuleFor(aec => aec.SalaryType)
            .NotNull().WithMessage("Salary type is required.")
            .IsInEnum();

        RuleFor(aec => aec.Department)
            .NotNull().WithMessage("Department is required.")
            .IsInEnum();

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