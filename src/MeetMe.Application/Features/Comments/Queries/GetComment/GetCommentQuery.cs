using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Comments.DTOs;
using MediatR;

namespace MeetMe.Application.Features.Comments.Queries.GetComment;

public record GetCommentQuery(int CommentId) : IRequest<Result<CommentDto>>;
