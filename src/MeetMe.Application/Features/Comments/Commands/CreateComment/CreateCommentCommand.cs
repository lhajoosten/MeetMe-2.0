using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Comments.Commands.CreateComment;

public record CreateCommentCommand : IRequest<Result<CommentDto>>
{
    public string Content { get; init; } = string.Empty;
    public int PostId { get; init; }
    public Guid AuthorId { get; init; }
    public int? ParentCommentId { get; init; }

    public CreateCommentCommand(string content, int postId, Guid authorId, int? parentCommentId = null)
    {
        Content = content;
        PostId = postId;
        AuthorId = authorId;
        ParentCommentId = parentCommentId;
    }
}
