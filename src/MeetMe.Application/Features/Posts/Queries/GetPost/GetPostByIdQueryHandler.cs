using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Posts.DTOs;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.Queries.GetPost;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, Result<PostDetailDto>>
{
    private readonly IQueryRepository<Post, int> _postRepository;
    private readonly IMapper _mapper;

    public GetPostByIdQueryHandler(IQueryRepository<Post, int> postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<Result<PostDetailDto>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken, 
                p => p.Author, p => p.Meeting, p => p.Comments);

            if (post == null)
            {
                return Result.Failure<PostDetailDto>("Post not found");
            }

            var postDto = _mapper.Map<PostDetailDto>(post);

            return Result.Success(postDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<PostDetailDto>($"Failed to get post: {ex.Message}");
        }
    }
}
