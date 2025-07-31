using MeetMe.Application.Common.Abstraction;

namespace MeetMe.Application.Features.Meetings.Commands.CreateMeeting
{
    public record CreateMeetingCommand : ICommand<Guid>
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public DateTime StartDateTime { get; init; }
        public DateTime EndDateTime { get; init; }
        public int? MaxAttendees { get; init; }
        public Guid CreatorId { get; init; }
    }
}
