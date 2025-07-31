using FluentValidation;

namespace MeetMe.Application.Features.Meetings.Commands.CreateMeeting
{
    public class CreateMeetingCommandValidator : AbstractValidator<CreateMeetingCommand>
    {
        public CreateMeetingCommandValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required")
                .MaximumLength(200)
                .WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description is required")
                .MaximumLength(2000)
                .WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Location)
                .NotEmpty()
                .WithMessage("Location is required")
                .MaximumLength(500)
                .WithMessage("Location cannot exceed 500 characters");

            RuleFor(x => x.StartDateTime)
                .NotEmpty()
                .WithMessage("Start date and time is required")
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Meeting cannot be scheduled in the past");

            RuleFor(x => x.EndDateTime)
                .NotEmpty()
                .WithMessage("End date and time is required")
                .GreaterThan(x => x.StartDateTime)
                .WithMessage("End date must be after start date");

            RuleFor(x => x.MaxAttendees)
                .GreaterThan(0)
                .When(x => x.MaxAttendees.HasValue)
                .WithMessage("Maximum attendees must be greater than 0");

            RuleFor(x => x.CreatorId)
                .NotEmpty()
                .WithMessage("Creator ID is required");
        }
    }
}
