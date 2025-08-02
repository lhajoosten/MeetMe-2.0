using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Features.Posts.DTOs;

namespace MeetMe.Application.Features.Posts.Commands.CreatePost;

public record CreatePostCommand : ICommand<PostDetailDto>
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int MeetingId { get; init; }
    public int AuthorId { get; init; }

    public CreatePostCommand(string title, string content, int meetingId, int authorId)
    {
        Title = title;
        Content = content;
        MeetingId = meetingId;
        AuthorId = authorId;
    }
}
