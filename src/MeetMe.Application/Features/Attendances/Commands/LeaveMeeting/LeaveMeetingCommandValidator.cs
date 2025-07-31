using FluentValidation;

namespace MeetMe.Application.Features.Attendances.Commands.LeaveMeeting;

public class LeaveMeetingCommandValidator : AbstractValidator<LeaveMeetingCommand>
{
    public LeaveMeetingCommandValidator()
    {
        RuleFor(x => x.MeetingId)
            .NotEmpty()
            .WithMessage("Meeting ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
