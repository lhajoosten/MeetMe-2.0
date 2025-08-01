using FluentValidation;

namespace MeetMe.Application.Features.Comments.Queries.GetCommentsByPost;

public class GetCommentsByPostQueryValidator : AbstractValidator<GetCommentsByPostQuery>
{
    public GetCommentsByPostQueryValidator()
    {
        RuleFor(x => x.PostId)
            .GreaterThan(0)
            .WithMessage("Post ID must be greater than 0");

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
