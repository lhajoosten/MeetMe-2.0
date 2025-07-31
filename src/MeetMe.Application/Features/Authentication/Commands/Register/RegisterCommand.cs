using MediatR;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Authentication.DTOs;

namespace MeetMe.Application.Features.Authentication.Commands;

public record RegisterCommand : IRequest<Result<AuthenticationResult>>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string ConfirmPassword { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;

    public RegisterCommand(string email, string password, string confirmPassword, string firstName, string lastName)
    {
        Email = email;
        Password = password;
        ConfirmPassword = confirmPassword;
        FirstName = firstName;
        LastName = lastName;
    }
}
