using AutoMapper;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Posts.Queries.GetPost;

public class GetPostByIdQueryHandler : IRequestHandler<GetPostByIdQuery, Result<PostDto>>
{
    private readonly IQueryRepository<Post, int> _postRepository;
    private readonly IMapper _mapper;

    public GetPostByIdQueryHandler(IQueryRepository<Post, int> postRepository, IMapper mapper)
    {
        _postRepository = postRepository;
        _mapper = mapper;
    }

    public async Task<Result<PostDto>> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var post = await _postRepository.GetByIdAsync(request.Id, cancellationToken, 
                p => p.Author, p => p.Meeting, p => p.Comments);

            if (post == null)
            {
                return Result.Failure<PostDto>("Post not found");
            }

            var postDto = new PostDto
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
            };

            return Result.Success(postDto);
        }
        catch (Exception ex)
        {
            return Result.Failure<PostDto>($"Error retrieving post: {ex.Message}");
        }
    }
}
