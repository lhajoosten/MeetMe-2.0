namespace MeetMe.Application.Features.Posts.DTOs
{
    /// <summary>
    /// DTO for creating a new post
    /// </summary>
    public record CreatePostDto
    {
        public string? Title { get; init; }
        public string Content { get; init; } = string.Empty;
        public int MeetingId { get; init; }
    }
}
