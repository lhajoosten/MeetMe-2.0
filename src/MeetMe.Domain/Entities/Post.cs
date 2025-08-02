using Ardalis.GuardClauses;
using MeetMe.Domain.Common;
using MeetMe.Domain.Events;

namespace MeetMe.Domain.Entities
{
    public class Post : BaseEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public int AuthorId { get; private set; }
        public int MeetingId { get; private set; }

        public User Author { get; private set; } = null!;
        public Meeting Meeting { get; private set; } = null!;
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

        private Post() { } // EF Core

        public static Post Create(string title, string content, User author, Meeting meeting)
        {
            Guard.Against.NullOrEmpty(title, nameof(title), "Title cannot be null or empty.");
            Guard.Against.NullOrEmpty(content, nameof(content), "Content cannot be null or empty.");
            Guard.Against.Null(author, nameof(author), "Author cannot be null when creating a post.");
            Guard.Against.Null(meeting, nameof(meeting), "Meeting cannot be null when creating a post.");

            var post = new Post
            {
                Title = title,
                Content = content,
                Author = author,
                AuthorId = author.Id,
                Meeting = meeting,
                MeetingId = meeting.Id
            };

            post.AddDomainEvent(new PostCreatedEvent(post));

            return post;
        }

        public void UpdateContent(string title, string content, User user)
        {
            Guard.Against.NullOrEmpty(title, nameof(title), "Title cannot be null or empty.");
            Guard.Against.NullOrEmpty(content, nameof(content), "Content cannot be null or empty.");
            Guard.Against.Null(user, nameof(user), "User cannot be null when updating a post.");

            Title = title;
            Content = content;
        
            LastModifiedDate = DateTime.Now;
            LastModifiedByUserId = user.Id;
        }

        public void Deactivate(User user)
        {
            Guard.Against.Null(user, nameof(user), "User cannot be null when deactivating a post.");

            IsActive = false;

            LastModifiedDate = DateTime.Now;
            LastModifiedByUserId = user.Id;
        }

        public int ActiveCommentsCount => Comments.Count(c => c.IsActive);

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

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
