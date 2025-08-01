using MeetMe.Application.Common.Models;
using MediatR;

namespace MeetMe.Application.Features.Comments.Queries.GetComment;

public record GetCommentQuery(int CommentId) : IRequest<Result<CommentDto>>;
