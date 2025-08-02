using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Comments.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Comments.Queries.GetCommentsByPost;

public class GetCommentsByPostQueryHandler : IRequestHandler<GetCommentsByPostQuery, Result<List<CommentDto>>>
{
    private readonly IQueryRepository<Comment, int> _commentQueryRepository;
    private readonly IQueryRepository<Post, int> _postQueryRepository;
    private readonly IMapper _mapper;

    public GetCommentsByPostQueryHandler(
        IQueryRepository<Comment, int> commentQueryRepository,
        IQueryRepository<Post, int> postQueryRepository,
        IMapper mapper)
    {
        _commentQueryRepository = commentQueryRepository;
        _postQueryRepository = postQueryRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<CommentDto>>> Handle(GetCommentsByPostQuery request, CancellationToken cancellationToken)
    {
        // Verify that the post exists
        var post = await _postQueryRepository.GetByIdAsync(request.PostId, cancellationToken);
        if (post == null || !post.IsActive)
        {
            return Result.Failure<List<CommentDto>>("Post not found");
        }

        // Get all comments for the post with author and replies included
        var allComments = await _commentQueryRepository.FindAsync(
            c => c.PostId == request.PostId && c.IsActive,
            cancellationToken,
            c => c.Author,
            c => c.Replies);

        // Get only top-level comments (not replies) for pagination
        var topLevelComments = allComments
            .Where(c => !c.ParentCommentId.HasValue)
            .OrderBy(c => c.CreatedDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var commentDtos = _mapper.Map<List<CommentDto>>(topLevelComments);
        return Result.Success(commentDtos);
    }
}
