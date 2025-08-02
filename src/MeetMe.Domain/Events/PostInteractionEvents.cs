using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;

namespace MeetMe.Domain.Events
{
    public class PostLikedEvent(PostLike postLike) : IDomainEvent
    {
        public PostLike PostLike { get; } = postLike ?? throw new ArgumentNullException(nameof(postLike));
        public DateTime OccurredOn { get; } = DateTime.Now;
    }

    public class PostUnlikedEvent(PostLike postLike) : IDomainEvent
    {
        public PostLike PostLike { get; } = postLike ?? throw new ArgumentNullException(nameof(postLike));
        public DateTime OccurredOn { get; } = DateTime.Now;
    }

    public class PostBookmarkedEvent(PostBookmark postBookmark) : IDomainEvent
    {
        public PostBookmark PostBookmark { get; } = postBookmark ?? throw new ArgumentNullException(nameof(postBookmark));
        public DateTime OccurredOn { get; } = DateTime.Now;
    }

    public class PostUnbookmarkedEvent(PostBookmark postBookmark) : IDomainEvent
    {
        public PostBookmark PostBookmark { get; } = postBookmark ?? throw new ArgumentNullException(nameof(postBookmark));
        public DateTime OccurredOn { get; } = DateTime.Now;
    }
}
