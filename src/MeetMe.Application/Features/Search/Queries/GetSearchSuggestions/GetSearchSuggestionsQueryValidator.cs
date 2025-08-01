using FluentValidation;

namespace MeetMe.Application.Features.Search.Queries.GetSearchSuggestions;

public class GetSearchSuggestionsQueryValidator : AbstractValidator<GetSearchSuggestionsQuery>
{
    public GetSearchSuggestionsQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Search query is required")
            .MinimumLength(1)
            .WithMessage("Search query must be at least 1 character long")
            .MaximumLength(100)
            .WithMessage("Search query cannot exceed 100 characters");

        RuleFor(x => x.MaxSuggestions)
            .GreaterThan(0)
            .WithMessage("Max suggestions must be greater than 0")
            .LessThanOrEqualTo(50)
            .WithMessage("Max suggestions cannot exceed 50");
    }
}
