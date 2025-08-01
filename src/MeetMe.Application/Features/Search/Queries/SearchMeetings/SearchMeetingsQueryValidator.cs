using FluentValidation;

namespace MeetMe.Application.Features.Search.Queries.SearchMeetings;

public class SearchMeetingsQueryValidator : AbstractValidator<SearchMeetingsQuery>
{
    public SearchMeetingsQueryValidator()
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
    }
}
