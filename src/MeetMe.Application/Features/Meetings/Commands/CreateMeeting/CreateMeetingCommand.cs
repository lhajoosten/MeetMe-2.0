using MeetMe.Application.Common.Abstraction;

namespace MeetMe.Application.Features.Meetings.Commands.CreateMeeting
{
    public record CreateMeetingCommand : ICommand<int>
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public DateTime StartDateTime { get; init; }
        public DateTime EndDateTime { get; init; }
        public int? MaxAttendees { get; init; }
        public bool IsPublic { get; init; }
        public int CreatorId { get; init; }
    }
}
