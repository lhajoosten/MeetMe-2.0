using FluentValidation;

namespace MeetMe.Application.Features.Search.Queries.GlobalSearch;

public class GlobalSearchQueryValidator : AbstractValidator<GlobalSearchQuery>
{
    public GlobalSearchQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Search query is required")
            .MinimumLength(2)
            .WithMessage("Search query must be at least 2 characters long")
            .MaximumLength(200)
            .WithMessage("Search query cannot exceed 200 characters");

        RuleFor(x => x.Page)
            .GreaterThan(0)
            .WithMessage("Page must be greater than 0");

        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .WithMessage("Page size must be greater than 0")
            .LessThanOrEqualTo(100)
            .WithMessage("Page size cannot exceed 100");

        When(x => x.Filters != null, () => {
            RuleFor(x => x.Filters!.SortBy)
                .Must(x => string.IsNullOrEmpty(x) || new[] { "Relevance", "Date", "Title" }.Contains(x))
                .WithMessage("SortBy must be one of: Relevance, Date, Title");

            RuleFor(x => x.Filters!.SortDirection)
                .Must(x => string.IsNullOrEmpty(x) || new[] { "Asc", "Desc" }.Contains(x))
                .WithMessage("SortDirection must be one of: Asc, Desc");

            When(x => x.Filters!.FromDate.HasValue && x.Filters.ToDate.HasValue, () => {
                RuleFor(x => x.Filters!.FromDate)
                    .LessThanOrEqualTo(x => x.Filters!.ToDate)
                    .WithMessage("FromDate must be less than or equal to ToDate");
            });
        });
    }
}
