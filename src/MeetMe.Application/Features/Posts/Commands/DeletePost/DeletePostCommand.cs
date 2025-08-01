using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Posts.Commands.DeletePost;

public record DeletePostCommand : IRequest<Result<bool>>
{
    public int Id { get; init; }
    public Guid UserId { get; init; }

    public DeletePostCommand(int id, Guid userId)
    {
        Id = id;
        UserId = userId;
    }
}
