using FluentValidation;

namespace MeetMe.Application.Features.Meetings.Commands.DeleteMeeting;

public class DeleteMeetingCommandValidator : AbstractValidator<DeleteMeetingCommand>
{
    public DeleteMeetingCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Meeting ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
