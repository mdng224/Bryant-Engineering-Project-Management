using FluentValidation;

namespace App.Application.Employees.Queries.ListEmployees;

public sealed class ListEmployeesQueryValidator : AbstractValidator<ListEmployeesQuery>
{
    public ListEmployeesQueryValidator()
    {
        RuleFor(q => q.PagedQuery)
            .NotNull();

        RuleFor(q => q.NameFilter)
            .MaximumLength(100);
    }
}