using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Users.Queries.GetUser;

public record GetUserByIdQuery : IRequest<Result<UserDto>>
{
    public Guid Id { get; init; }

    public GetUserByIdQuery(Guid id)
    {
        Id = id;
    }
}
