using MeetMe.Application.Common.Models;
using MediatR;

namespace MeetMe.Application.Features.Comments.Commands.DeleteComment;

public record DeleteCommentCommand(
    int CommentId,
    Guid UserId
) : IRequest<Result<bool>>;
