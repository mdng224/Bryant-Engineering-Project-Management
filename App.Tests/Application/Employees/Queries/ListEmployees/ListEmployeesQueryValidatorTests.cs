using App.Application.Common.Pagination;
using App.Application.Employees.Queries.ListEmployees;
using FluentValidation.TestHelper;

namespace App.Tests.Application.Employees.Queries.ListEmployees;

public class ListEmployeesQueryValidatorTests
{
    private readonly ListEmployeesQueryValidator _validator = new();

    private static ListEmployeesQuery CreateValidQuery() =>
        new(
            PagedQuery: new PagedQuery(page: 1, pageSize: 25),
            NameFilter: "John",
            IsDeleted: null
        );

    [Fact]
    public void ValidQuery_Should_PassValidation()
    {
        var query = CreateValidQuery();

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void PagedQuery_Null_Should_HaveError()
    {
        var query = new ListEmployeesQuery(
            PagedQuery: null!,
            NameFilter: "John",
            IsDeleted: false
        );

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.PagedQuery);
    }

    [Fact]
    public void NameFilter_Null_Should_PassValidation()
    {
        var query = CreateValidQuery() with { NameFilter = null };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(q => q.NameFilter);
    }

    [Fact]
    public void NameFilter_Empty_Should_PassValidation()
    {
        var query = CreateValidQuery() with { NameFilter = string.Empty };

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(q => q.NameFilter);
    }

    [Fact]
    public void NameFilter_TooLong_Should_HaveError()
    {
        var tooLong = new string('a', 101);
        var query = CreateValidQuery() with { NameFilter = tooLong };

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.NameFilter);
    }
}