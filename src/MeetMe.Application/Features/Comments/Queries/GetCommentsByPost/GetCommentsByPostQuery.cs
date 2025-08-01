using MeetMe.Application.Common.Models;
using MediatR;

namespace MeetMe.Application.Features.Comments.Queries.GetCommentsByPost;

public record GetCommentsByPostQuery(
    int PostId,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<List<CommentDto>>>;
