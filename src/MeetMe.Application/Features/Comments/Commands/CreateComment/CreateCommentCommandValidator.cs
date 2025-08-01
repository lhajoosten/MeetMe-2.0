using FluentValidation;

namespace MeetMe.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
{
    public CreateCommentCommandValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(1000)
            .WithMessage("Content cannot exceed 1000 characters");

        RuleFor(x => x.PostId)
            .GreaterThan(0)
            .WithMessage("Valid Post ID is required");

        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .WithMessage("Author ID is required");

        RuleFor(x => x.ParentCommentId)
            .GreaterThan(0)
            .When(x => x.ParentCommentId.HasValue)
            .WithMessage("Parent Comment ID must be greater than 0 when specified");
    }
}
