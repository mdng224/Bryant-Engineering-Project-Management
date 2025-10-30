using FluentValidation;

namespace App.Api.Common.Pagination;

public sealed class PagedRequestValidator : AbstractValidator<PagedRequest>
{
    private const int MaxPageSize = 100;

    public PagedRequestValidator()
    {
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, MaxPageSize);
    }
}