using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Features.Comments.DTOs;

namespace MeetMe.Application.Features.Comments.Commands.CreateComment;

public record CreateCommentCommand : ICommand<CommentDto>
{
    public string Content { get; init; } = string.Empty;
    public int PostId { get; init; }
    public int AuthorId { get; init; }
    public int? ParentCommentId { get; init; }

    public CreateCommentCommand(string content, int postId, int authorId, int? parentCommentId = null)
    {
        Content = content;
        PostId = postId;
        AuthorId = authorId;
        ParentCommentId = parentCommentId;
    }
}
