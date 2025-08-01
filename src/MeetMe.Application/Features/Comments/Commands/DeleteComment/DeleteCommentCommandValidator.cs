using FluentValidation;

namespace MeetMe.Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandValidator : AbstractValidator<DeleteCommentCommand>
{
    public DeleteCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .GreaterThan(0)
            .WithMessage("Comment ID must be greater than 0");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
