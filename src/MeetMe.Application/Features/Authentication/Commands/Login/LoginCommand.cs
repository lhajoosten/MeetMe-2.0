using MediatR;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Authentication.DTOs;

namespace MeetMe.Application.Features.Authentication.Commands.Login;

public record LoginCommand : IRequest<Result<AuthenticationResult>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    public LoginCommand(string email, string password)
    {
        Email = email;
        Password = password;
    }
}
