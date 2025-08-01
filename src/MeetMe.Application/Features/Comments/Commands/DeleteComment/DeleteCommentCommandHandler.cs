using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Comments.Commands.DeleteComment;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, Result<bool>>
{
    private readonly ICommandRepository<Comment, int> _commentCommandRepository;
    private readonly IQueryRepository<Comment, int> _commentQueryRepository;
    private readonly IQueryRepository<User, Guid> _userQueryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(
        ICommandRepository<Comment, int> commentCommandRepository,
        IQueryRepository<Comment, int> commentQueryRepository,
        IQueryRepository<User, Guid> userQueryRepository,
        IUnitOfWork unitOfWork)
    {
        _commentCommandRepository = commentCommandRepository;
        _commentQueryRepository = commentQueryRepository;
        _userQueryRepository = userQueryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _commentQueryRepository.GetByIdAsync(request.CommentId, cancellationToken);
        if (comment == null)
        {
            return Result.Failure<bool>("Comment not found");
        }

        // Check if the current user is the author of the comment
        if (comment.AuthorId != request.UserId)
        {
            return Result.Failure<bool>("You can only delete your own comments");
        }

        // Get the user entity for domain method
        var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user == null)
        {
            return Result.Failure<bool>("User not found");
        }

        // Use domain method to deactivate (soft delete)
        comment.Deactivate(user);

        await _commentCommandRepository.SoftDeleteAsync(comment, request.UserId.ToString(), cancellationToken);
        await _unitOfWork.SaveChangesAsync(request.UserId.ToString(), cancellationToken);

        return Result.Success(true);
    }
}
