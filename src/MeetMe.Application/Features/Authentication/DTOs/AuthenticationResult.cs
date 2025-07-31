using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Authentication.DTOs;

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
    public List<string> Errors { get; set; } = new();
}
