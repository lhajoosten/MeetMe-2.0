using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;

namespace MeetMe.Domain.Events
{
    public class UserJoinedMeetingEvent(Attendance attendance) : IDomainEvent
    {
        public Attendance Attendance { get; } = attendance ?? throw new ArgumentNullException(nameof(attendance), "Attendance cannot be null when creating a UserJoinedMeetingEvent.");
        public DateTime OccurredOn { get; } = DateTime.Now;
    }
}