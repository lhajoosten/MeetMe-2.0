namespace MeetMe.Application.Common.Models
{
    public record PostDto
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Content { get; init; } = string.Empty;
        public Guid AuthorId { get; init; }
        public string AuthorName { get; init; } = string.Empty;
        public Guid MeetingId { get; init; }
        public bool IsActive { get; init; }
        public int CommentCount { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? LastModifiedDate { get; init; }
    }
}
