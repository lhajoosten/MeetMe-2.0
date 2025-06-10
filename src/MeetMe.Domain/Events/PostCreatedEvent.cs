using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;

namespace MeetMe.Domain.Events;

public class PostCreatedEvent(Post post) : IDomainEvent
{
    public Post Post { get; } = post ?? throw new ArgumentNullException(nameof(post), "Post cannot be null.");
    public DateTime OccurredOn { get; } = DateTime.Now;
}