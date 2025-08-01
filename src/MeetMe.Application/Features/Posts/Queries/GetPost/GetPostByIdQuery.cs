using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Posts.Queries.GetPost;

public record GetPostByIdQuery : IRequest<Result<PostDto>>
{
    public int Id { get; init; }

    public GetPostByIdQuery(int id)
    {
        Id = id;
    }
}
