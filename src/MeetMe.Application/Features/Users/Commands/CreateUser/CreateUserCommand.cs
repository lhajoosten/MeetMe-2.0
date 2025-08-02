using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Users.Commands.CreateUser;

public record CreateUserCommand : ICommand<int>
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Bio { get; init; }
}
