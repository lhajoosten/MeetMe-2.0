using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandHandler : IRequestHandler<UpdatePostCommand, Result<PostDto>>
{
    private readonly ICommandRepository<Post, int> _postCommandRepository;
    private readonly IQueryRepository<Post, int> _postQueryRepository;
    private readonly IQueryRepository<User, Guid> _userQueryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdatePostCommandHandler(
        ICommandRepository<Post, int> postCommandRepository,
        IQueryRepository<Post, int> postQueryRepository,
        IQueryRepository<User, Guid> userQueryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _postCommandRepository = postCommandRepository;
        _postQueryRepository = postQueryRepository;
        _userQueryRepository = userQueryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PostDto>> Handle(UpdatePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the post
            var post = await _postQueryRepository.GetByIdAsync(request.Id, cancellationToken);
            if (post == null)
            {
                return Result.Failure<PostDto>("Post not found");
            }

            // Get the user
            var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<PostDto>("User not found");
            }

            // Check if user is the author or has permission to edit
            if (post.AuthorId != request.UserId)
            {
                return Result.Failure<PostDto>("You don't have permission to update this post");
            }

            // Update the post
            post.UpdateContent(request.Title, request.Content, user);

            await _postCommandRepository.UpdateAsync(post, request.UserId.ToString(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(request.UserId.ToString(), cancellationToken);

            var postDto = _mapper.Map<PostDto>(post);

            return Result.Success(postDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<PostDto>($"Failed to update post: {ex.Message}");
        }
    }
}
