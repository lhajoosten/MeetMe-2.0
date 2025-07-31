using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand : IRequest<Result<Guid>>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Bio { get; init; }
}
