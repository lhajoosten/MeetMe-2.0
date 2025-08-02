namespace MeetMe.Application.Features.Users.DTOs
{
    /// <summary>
    /// DTO for updating user information
    /// </summary>
    public record UpdateUserDto
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string? Bio { get; init; }
        public string? ProfilePictureUrl { get; init; }
    }
}
