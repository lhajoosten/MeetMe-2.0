using FluentValidation;

namespace MeetMe.Application.Features.Comments.Commands.UpdateComment;

public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
{
    public UpdateCommentCommandValidator()
    {
        RuleFor(x => x.CommentId)
            .GreaterThan(0)
            .WithMessage("Comment ID must be greater than 0");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(1000)
            .WithMessage("Content cannot exceed 1000 characters");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
