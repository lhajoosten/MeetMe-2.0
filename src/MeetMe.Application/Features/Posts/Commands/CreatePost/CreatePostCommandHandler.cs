using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Result<PostDto>>
{
    private readonly ICommandRepository<Post, int> _postCommandRepository;
    private readonly IQueryRepository<User, Guid> _userQueryRepository;
    private readonly IQueryRepository<Meeting, Guid> _meetingQueryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePostCommandHandler(
        ICommandRepository<Post, int> postCommandRepository,
        IQueryRepository<User, Guid> userQueryRepository,
        IQueryRepository<Meeting, Guid> meetingQueryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _postCommandRepository = postCommandRepository;
        _userQueryRepository = userQueryRepository;
        _meetingQueryRepository = meetingQueryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PostDto>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate that the author exists
            var author = await _userQueryRepository.GetByIdAsync(request.AuthorId, cancellationToken);
            if (author == null)
            {
                return Result.Failure<PostDto>("Author not found");
            }

            // Validate that the meeting exists
            var meeting = await _meetingQueryRepository.GetByIdAsync(request.MeetingId, cancellationToken);
            if (meeting == null)
            {
                return Result.Failure<PostDto>("Meeting not found");
            }

            // Create the post
            var post = Post.Create(request.Title, request.Content, author, meeting);

            await _postCommandRepository.AddAsync(post, request.AuthorId.ToString(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(request.AuthorId.ToString(), cancellationToken);

            var postDto = _mapper.Map<PostDto>(post);

            return Result.Success(postDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<PostDto>($"Failed to create post: {ex.Message}");
        }
    }
}
