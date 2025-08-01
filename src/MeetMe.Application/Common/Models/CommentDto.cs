namespace MeetMe.Application.Common.Models
{
    public record CommentDto
    {
        public int Id { get; init; }
        public string Content { get; init; } = string.Empty;
        public Guid AuthorId { get; init; }
        public string AuthorName { get; init; } = string.Empty;
        public int PostId { get; init; }
        public int? ParentCommentId { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedDate { get; init; }
        public DateTime? LastModifiedDate { get; init; }
        public List<CommentDto> Replies { get; init; } = new();
    }
}
