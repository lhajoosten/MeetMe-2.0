using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Comments.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Comments.Queries.GetComment;

public class GetCommentQueryHandler : IRequestHandler<GetCommentQuery, Result<CommentDto>>
{
    private readonly IQueryRepository<Comment, int> _commentQueryRepository;
    private readonly IMapper _mapper;

    public GetCommentQueryHandler(
        IQueryRepository<Comment, int> commentQueryRepository,
        IMapper mapper)
    {
        _commentQueryRepository = commentQueryRepository;
        _mapper = mapper;
    }

    public async Task<Result<CommentDto>> Handle(GetCommentQuery request, CancellationToken cancellationToken)
    {
        var comment = await _commentQueryRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment == null || !comment.IsActive)
        {
            return Result.Failure<CommentDto>("Comment not found");
        }

        var commentDto = _mapper.Map<CommentDto>(comment);
        return Result.Success(commentDto);
    }
}
