using MeetMe.Application.Common.Models;
using MeetMe.Application.Common.Abstraction;
using MediatR;

namespace MeetMe.Application.Features.Comments.Commands.UpdateComment;

public record UpdateCommentCommand(
    int CommentId,
    string Content,
    int UserId
) : IRequest<Result<bool>>;
