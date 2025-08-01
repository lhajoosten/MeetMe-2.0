using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Posts.Commands.CreatePost;

public record CreatePostCommand : IRequest<Result<PostDto>>
{
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid MeetingId { get; init; }
    public Guid AuthorId { get; init; }

    public CreatePostCommand(string title, string content, Guid meetingId, Guid authorId)
    {
        Title = title;
        Content = content;
        MeetingId = meetingId;
        AuthorId = authorId;
    }
}
