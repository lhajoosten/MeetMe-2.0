namespace MeetMe.Application.Features.Posts.DTOs
{
    /// <summary>
    /// DTO for updating a post
    /// </summary>
    public record UpdatePostDto
    {
        public int Id { get; init; }
        public string? Title { get; init; }
        public string Content { get; init; } = string.Empty;
    }
}
