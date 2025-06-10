using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;

namespace MeetMe.Domain.Events
{
    public class CommentCreatedEvent(Comment comment) : IDomainEvent
    {
        public Comment Comment { get; } = comment ?? throw new ArgumentNullException(nameof(comment), "Comment cannot be null when creating a CommentCreatedEvent.");
        public DateTime OccurredOn { get; } = DateTime.Now;
    }
}