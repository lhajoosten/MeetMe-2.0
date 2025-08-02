using Ardalis.GuardClauses;
using MeetMe.Domain.Common;
using MeetMe.Domain.Events;

namespace MeetMe.Domain.Entities
{
    public class PostLike : BaseEntity
    {
        public int UserId { get; private set; }
        public int PostId { get; private set; }

        public User User { get; private set; } = null!;
        public Post Post { get; private set; } = null!;

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        private PostLike() { } // EF Core

        public static PostLike Create(User user, Post post)
        {
            Guard.Against.Null(user, nameof(user), "User cannot be null when creating a like.");
            Guard.Against.Null(post, nameof(post), "Post cannot be null when creating a like.");

            var like = new PostLike
            {
                User = user,
                UserId = user.Id,
                Post = post,
                PostId = post.Id
            };

            like.AddDomainEvent(new PostLikedEvent(like));

            return like;
        }

        public void Remove(User user)
        {
            Guard.Against.Null(user, nameof(user), "User cannot be null when removing a like.");

            this.AddDomainEvent(new PostUnlikedEvent(this));
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
