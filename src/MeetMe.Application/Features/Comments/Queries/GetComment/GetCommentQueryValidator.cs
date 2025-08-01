using FluentValidation;

namespace MeetMe.Application.Features.Comments.Queries.GetComment;

public class GetCommentQueryValidator : AbstractValidator<GetCommentQuery>
{
    public GetCommentQueryValidator()
    {
        RuleFor(x => x.CommentId)
            .GreaterThan(0)
            .WithMessage("Comment ID must be greater than 0");
    }
}
