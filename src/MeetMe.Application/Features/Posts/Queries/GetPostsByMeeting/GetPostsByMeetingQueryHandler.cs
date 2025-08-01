using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.Queries.GetPostsByMeeting;

public class GetPostsByMeetingQueryHandler : IRequestHandler<GetPostsByMeetingQuery, Result<List<PostDto>>>
{
    private readonly IQueryRepository<Post, int> _postRepository;
    private readonly IMapper _mapper;

    public GetPostsByMeetingQueryHandler(IQueryRepository<Post, int> postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<PostDto>>> Handle(GetPostsByMeetingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var posts = await _postRepository.FindAsync(
                p => p.MeetingId == request.MeetingId && p.IsActive,
                cancellationToken,
                p => p.Author, p => p.Meeting, p => p.Comments);

            var postDtos = posts.Select(post => new PostDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                AuthorId = post.AuthorId,
                AuthorName = post.Author.FullName,
                MeetingId = post.MeetingId,
                IsActive = post.IsActive,
                CommentCount = post.Comments.Count,
                CreatedDate = post.CreatedDate,
                LastModifiedDate = post.LastModifiedDate
            }).OrderByDescending(p => p.CreatedDate).ToList();

            return Result.Success(postDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<PostDto>>($"Error retrieving posts: {ex.Message}");
        }
    }
}
