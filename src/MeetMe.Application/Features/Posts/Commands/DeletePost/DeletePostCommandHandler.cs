using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.Commands.DeletePost;

public class DeletePostCommandHandler : IRequestHandler<DeletePostCommand, Result<bool>>
{
    private readonly ICommandRepository<Post, int> _postCommandRepository;
    private readonly IQueryRepository<Post, int> _postQueryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeletePostCommandHandler(
        ICommandRepository<Post, int> postCommandRepository,
        IQueryRepository<Post, int> postQueryRepository,
        IUnitOfWork unitOfWork)
    {
        _postCommandRepository = postCommandRepository;
        _postQueryRepository = postQueryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeletePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the post
            var post = await _postQueryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (post == null)
            {
                return Result.Failure<bool>("Post not found");
            }

            // Check if user is the author or has permission to delete
            if (post.AuthorId != request.UserId)
            {
                return Result.Failure<bool>("You don't have permission to delete this post");
            }

            // Soft delete the post
            await _postCommandRepository.SoftDeleteAsync(post, request.UserId.ToString(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(request.UserId.ToString(), cancellationToken);

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>($"Failed to delete post: {ex.Message}");
        }
    }
}
