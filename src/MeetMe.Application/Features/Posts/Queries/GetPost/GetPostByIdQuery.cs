using MediatR;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Posts.DTOs;

namespace MeetMe.Application.Features.Posts.Queries.GetPost;

public record GetPostByIdQuery : IRequest<Result<PostDetailDto>>
{
    public int Id { get; init; }

    public GetPostByIdQuery(int id)
    {
        Id = id;
    }
}
