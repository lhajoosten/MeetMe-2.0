namespace MeetMe.Application.Features.Users.DTOs
{
    /// <summary>
    /// DTO for creating a new user
    /// </summary>
    public record CreateUserDto
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? Bio { get; init; }
        public string? ProfilePictureUrl { get; init; }
    }
}