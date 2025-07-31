namespace MeetMe.Application.Common.Models
{
    public record CommentDto
    {
        public Guid Id { get; init; }
        public string Content { get; init; } = string.Empty;
        public Guid AuthorId { get; init; }
        public string AuthorName { get; init; } = string.Empty;
        public Guid PostId { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? LastModifiedDate { get; init; }
    }
}
