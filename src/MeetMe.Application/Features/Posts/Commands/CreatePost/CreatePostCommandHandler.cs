using AutoMapper;
using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Posts.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, PostDetailDto>
{
    private readonly ICommandRepository<Post, int> _postCommandRepository;
    private readonly IQueryRepository<Post, int> _postQueryRepository;
    private readonly IQueryRepository<User, int> _userQueryRepository;
    private readonly IQueryRepository<Meeting, int> _meetingQueryRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreatePostCommandHandler(
        ICommandRepository<Post, int> postCommandRepository,
        IQueryRepository<Post, int> postQueryRepository,
        IQueryRepository<User, int> userQueryRepository,
        IQueryRepository<Meeting, int> meetingQueryRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _postCommandRepository = postCommandRepository;
        _postQueryRepository = postQueryRepository;
        _userQueryRepository = userQueryRepository;
        _meetingQueryRepository = meetingQueryRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PostDetailDto>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate that the author exists
            var author = await _userQueryRepository.GetByIdAsync(request.AuthorId, cancellationToken);
            if (author == null)
            {
                return Result.Failure<PostDetailDto>("Author not found");
            }

            // Validate that the meeting exists
            var meeting = await _meetingQueryRepository.GetByIdAsync(request.MeetingId, cancellationToken);
            if (meeting == null)
            {
                return Result.Failure<PostDetailDto>("Meeting not found");
            }

            // Auto-generate title if not provided
            var title = string.IsNullOrWhiteSpace(request.Title) 
                ? GenerateTitleFromContent(request.Content) 
                : request.Title;

            // Create the post
            var post = Post.Create(title, request.Content, author, meeting);

            await _postCommandRepository.AddAsync(post, request.AuthorId, cancellationToken);
                        await _unitOfWork.SaveChangesAsync(request.AuthorId, cancellationToken);

            // Load the post with related data for mapping
            var createdPost = await _postQueryRepository.GetByIdAsync(post.Id, cancellationToken, p => p.Author, p => p.Meeting, p => p.Comments);
            var postDto = _mapper.Map<PostDetailDto>(createdPost);

            return Result.Success(postDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<PostDetailDto>($"Failed to create post: {ex.Message}");
        }
    }

    private static string GenerateTitleFromContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return "Untitled Post";

        // Take first 50 characters and add ellipsis if needed
        var title = content.Length > 50 
            ? content.Substring(0, 50).Trim() + "..." 
            : content.Trim();

        // Remove newlines and extra spaces
        title = System.Text.RegularExpressions.Regex.Replace(title, @"\s+", " ");

        return title;
    }
}
