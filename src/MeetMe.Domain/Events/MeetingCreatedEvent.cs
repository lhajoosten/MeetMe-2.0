using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;

namespace MeetMe.Domain.Events
{
    public class MeetingCreatedEvent(Meeting meeting) : IDomainEvent
    {
        public Meeting Meeting { get; } = meeting ?? throw new ArgumentNullException(nameof(meeting), "Meeting cannot be null.");
        public DateTime OccurredOn { get; } = DateTime.Now;
    }
}