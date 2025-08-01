using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Posts.Commands.UpdatePost;

public record UpdatePostCommand : IRequest<Result<PostDto>>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid UserId { get; init; }

    public UpdatePostCommand(int id, string title, string content, Guid userId)
    {
        Id = id;
        Title = title;
        Content = content;
        UserId = userId;
    }
}
