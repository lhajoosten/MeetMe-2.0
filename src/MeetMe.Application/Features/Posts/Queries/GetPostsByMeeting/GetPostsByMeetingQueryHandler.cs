using AutoMapper;
using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Posts.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.Queries.GetPostsByMeeting;

public class GetPostsByMeetingQueryHandler : IQueryHandler<GetPostsByMeetingQuery, List<PostDetailDto>>
{
    private readonly IQueryRepository<Post, int> _postRepository;
    private readonly IMapper _mapper;

    public GetPostsByMeetingQueryHandler(IQueryRepository<Post, int> postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<Result<List<PostDetailDto>>> Handle(GetPostsByMeetingQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var posts = await _postRepository.FindAsync(
                p => p.MeetingId == request.MeetingId && p.IsActive,
                cancellationToken,
                p => p.Author, p => p.Meeting, p => p.Comments);

            var postDtos = _mapper.Map<List<PostDetailDto>>(posts.OrderByDescending(p => p.CreatedDate));

            return Result.Success(postDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<PostDetailDto>>($"Failed to get posts: {ex.Message}");
        }
    }
}
