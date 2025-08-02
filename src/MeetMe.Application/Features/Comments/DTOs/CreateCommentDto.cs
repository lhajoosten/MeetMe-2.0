namespace MeetMe.Application.Features.Comments.DTOs
{
    /// <summary>
    /// DTO for creating a new comment
    /// </summary>
    public record CreateCommentDto
    {
        public string Content { get; init; } = string.Empty;
        public int PostId { get; init; }
        public int? ParentCommentId { get; init; }
    }
}
