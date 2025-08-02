namespace MeetMe.Application.Features.Comments.DTOs
{
    /// <summary>
    /// DTO for updating an existing comment
    /// </summary>
    public record UpdateCommentDto
    {
        public string Content { get; init; } = string.Empty;
    }
}
