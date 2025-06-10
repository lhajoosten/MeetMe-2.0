using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;

namespace MeetMe.Domain.Events
{
    public record MeetingCreatedEvent(Meeting Meeting) : IDomainEvent
    {
        public DateTime OccurredOn { get; } = DateTime.Now;
    }
}