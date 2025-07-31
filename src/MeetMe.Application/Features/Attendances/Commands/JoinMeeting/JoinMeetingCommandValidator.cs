using FluentValidation;

namespace MeetMe.Application.Features.Attendances.Commands.JoinMeeting;

public class JoinMeetingCommandValidator : AbstractValidator<JoinMeetingCommand>
{
    public JoinMeetingCommandValidator()
    {
        RuleFor(x => x.MeetingId)
            .NotEmpty()
            .WithMessage("Meeting ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
