using FluentValidation;

namespace MeetMe.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .WithMessage("Title cannot exceed 200 characters");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(5000)
            .WithMessage("Content cannot exceed 5000 characters");

        RuleFor(x => x.MeetingId)
            .NotEmpty()
            .WithMessage("Meeting ID is required");

        RuleFor(x => x.AuthorId)
            .NotEmpty()
            .WithMessage("Author ID is required");
    }
}
