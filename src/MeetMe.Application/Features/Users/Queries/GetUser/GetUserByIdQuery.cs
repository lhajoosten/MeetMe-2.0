using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Features.Users.DTOs;

namespace MeetMe.Application.Features.Users.Queries.GetUser;

public record GetUserByIdQuery : IQuery<UserDetailDto>
{
    public int Id { get; init; }

    public GetUserByIdQuery(int id)
    {
        Id = id;
    }
}
