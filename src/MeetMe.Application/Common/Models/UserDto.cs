namespace MeetMe.Application.Common.Models
{
    public record UserDto
    {
        public Guid Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string? Bio { get; init; }
        public string? ProfilePictureUrl { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedDate { get; init; }
    }
}
