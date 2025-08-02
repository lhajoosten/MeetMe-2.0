using MeetMe.Application.Common.Models;
using MediatR;

namespace MeetMe.Application.Features.Comments.Commands.DeleteComment;

public record DeleteCommentCommand(
    int CommentId,
    int UserId
) : IRequest<Result<bool>>;
