using Ardalis.GuardClauses;
using MeetMe.Domain.Common;
using MeetMe.Domain.Events;

namespace MeetMe.Domain.Entities
{
    public class Comment : BaseEntity
    {
        public string Content { get; private set; } = string.Empty;
        public Guid AuthorId { get; private set; }
        public Guid PostId { get; private set; }
        public Guid? ParentCommentId { get; private set; }
        public bool IsActive { get; private set; } = true;

        public User Author { get; private set; } = null!;
        public Post Post { get; private set; } = null!;
        public Comment? ParentComment { get; private set; }
        public ICollection<Comment> Replies { get; private set; } = new List<Comment>();

        private Comment() { } // EF Core

        public static Comment Create(string content, User author, Post post, Comment? parentComment = null)
        {
            Guard.Against.NullOrEmpty(content, nameof(content), "Content cannot be null or empty.");
            Guard.Against.Null(author, nameof(author), "Author cannot be null when creating a comment.");
            Guard.Against.Null(post, nameof(post), "Post cannot be null when creating a comment.");

            var comment = new Comment
            {
                Content = content,
                Author = author,
                Post = post,
                ParentComment = parentComment,
            };

            comment.AddDomainEvent(new CommentCreatedEvent(comment));

            return comment;
        }

        public void UpdateContent(string content, User user)
        {
            Guard.Against.NullOrEmpty(content, nameof(content), "Content cannot be null or empty.");
            Guard.Against.Null(user, nameof(user), "User cannot be null when updating a comment.");

            Content = content;
            LastModifiedDate = DateTime.Now;
            LastModifiedByUserId = user.Id;
        }

        public void Deactivate(User user)
        {
            Guard.Against.Null(user, nameof(user), "User cannot be null when deactivating a comment.");

            IsActive = false;
            LastModifiedDate = DateTime.Now;
            LastModifiedByUserId = user.Id;
        }

        public bool IsReply => ParentCommentId.HasValue;
        public int ActiveRepliesCount => Replies.Count(r => r.IsActive);

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