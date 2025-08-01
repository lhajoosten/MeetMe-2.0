using MeetMe.Application.Common.Models;
using MediatR;

namespace MeetMe.Application.Features.Comments.Commands.UpdateComment;

public record UpdateCommentCommand(
    int CommentId,
    string Content,
    Guid UserId
) : IRequest<Result<bool>>;
