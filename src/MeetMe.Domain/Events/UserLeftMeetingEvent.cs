using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;

namespace MeetMe.Domain.Events
{
    public class UserLeftMeetingEvent(Attendance attendance) : IDomainEvent
    {
        public Attendance Attendance { get; } = attendance ?? throw new ArgumentNullException(nameof(attendance), "Attendance cannot be null when creating a UserLeftMeetingEvent.");
        public DateTime OccurredOn { get; } = DateTime.Now;
    }
}