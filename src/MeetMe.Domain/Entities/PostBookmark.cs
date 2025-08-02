using Ardalis.GuardClauses;
using MeetMe.Domain.Common;
using MeetMe.Domain.Events;

namespace MeetMe.Domain.Entities
{
    public class PostBookmark : BaseEntity
    {
        public int UserId { get; private set; }
        public int PostId { get; private set; }

        public User User { get; private set; } = null!;
        public Post Post { get; private set; } = null!;

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private PostBookmark() { } // EF Core

        public static PostBookmark Create(User user, Post post)
        {
            Guard.Against.Null(user, nameof(user), "User cannot be null when creating a bookmark.");
            Guard.Against.Null(post, nameof(post), "Post cannot be null when creating a bookmark.");

            var bookmark = new PostBookmark
            {
                User = user,
                UserId = user.Id,
                Post = post,
                PostId = post.Id
            };

            bookmark.AddDomainEvent(new PostBookmarkedEvent(bookmark));

            return bookmark;
        }

        public void Remove(User user)
        {
            Guard.Against.Null(user, nameof(user), "User cannot be null when removing a bookmark.");

            this.AddDomainEvent(new PostUnbookmarkedEvent(this));
        }

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
