using FluentValidation;

namespace MeetMe.Application.Features.Meetings.Commands.UpdateMeeting;

public class UpdateMeetingCommandValidator : AbstractValidator<UpdateMeetingCommand>
{
    public UpdateMeetingCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Meeting ID is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.StartDateTime)
            .NotEmpty()
            .WithMessage("Start date and time is required")
            .GreaterThan(DateTime.Now)
            .WithMessage("Start date must be in the future");

        RuleFor(x => x.EndDateTime)
            .NotEmpty()
            .WithMessage("End date and time is required")
            .GreaterThan(x => x.StartDateTime)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage("Location is required")
            .MaximumLength(500)
            .WithMessage("Location must not exceed 500 characters");

        RuleFor(x => x.MaxAttendees)
            .GreaterThan(0)
            .WithMessage("Maximum attendees must be greater than 0")
            .LessThanOrEqualTo(1000)
            .WithMessage("Maximum attendees cannot exceed 1000");

        RuleFor(x => x.OrganizerId)
            .NotEmpty()
            .WithMessage("Organizer ID is required");
    }
}
