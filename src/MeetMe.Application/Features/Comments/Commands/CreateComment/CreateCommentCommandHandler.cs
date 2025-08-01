using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Comments.Commands.CreateComment;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, Result<CommentDto>>
{
    private readonly ICommandRepository<Comment, int> _commentCommandRepository;
    private readonly IQueryRepository<User, Guid> _userQueryRepository;
    private readonly IQueryRepository<Post, int> _postQueryRepository;
    private readonly IQueryRepository<Comment, int> _commentQueryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateCommentCommandHandler(
        ICommandRepository<Comment, int> commentCommandRepository,
        IQueryRepository<User, Guid> userQueryRepository,
        IQueryRepository<Post, int> postQueryRepository,
        IQueryRepository<Comment, int> commentQueryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _commentCommandRepository = commentCommandRepository;
        _userQueryRepository = userQueryRepository;
        _postQueryRepository = postQueryRepository;
        _commentQueryRepository = commentQueryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate that the author exists
            var author = await _userQueryRepository.GetByIdAsync(request.AuthorId, cancellationToken);
            if (author == null)
            {
                return Result.Failure<CommentDto>("Author not found");
            }

            // Validate that the post exists
            var post = await _postQueryRepository.GetByIdAsync(request.PostId, cancellationToken);
            if (post == null)
            {
                return Result.Failure<CommentDto>("Post not found");
            }

            // Validate parent comment if specified
            Comment? parentComment = null;
            if (request.ParentCommentId.HasValue)
            {
                parentComment = await _commentQueryRepository.GetByIdAsync(request.ParentCommentId.Value, cancellationToken);
                if (parentComment == null)
                {
                    return Result.Failure<CommentDto>("Parent comment not found");
                }

                // Ensure parent comment belongs to the same post
                if (parentComment.PostId != request.PostId)
                {
                    return Result.Failure<CommentDto>("Parent comment does not belong to the same post");
                }
            }

            // Create the comment
            var comment = Comment.Create(request.Content, author, post, parentComment);

            await _commentCommandRepository.AddAsync(comment, request.AuthorId.ToString(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(request.AuthorId.ToString(), cancellationToken);

            var commentDto = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                AuthorId = comment.AuthorId,
                AuthorName = author.FullName,
                PostId = comment.PostId,
                ParentCommentId = comment.ParentCommentId,
                IsActive = comment.IsActive,
                CreatedDate = comment.CreatedDate,
                LastModifiedDate = comment.LastModifiedDate
            };

            return Result.Success(commentDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<CommentDto>($"Failed to create comment: {ex.Message}");
        }
    }
}
