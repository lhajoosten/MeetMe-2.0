using Ardalis.GuardClauses;
using MeetMe.Domain.Common;

namespace MeetMe.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public Email Email { get; private set; } = null!;
        public string? Bio { get; private set; }
        public string? ProfilePictureUrl { get; private set; }
        public bool IsActive { get; private set; } = true;

        public ICollection<Meeting> CreatedMeetings { get; private set; } = new List<Meeting>();
        public ICollection<Attendance> Attendances { get; private set; } = new List<Attendance>();
        public ICollection<Post> Posts { get; private set; } = new List<Post>();
        public ICollection<Comment> Comments { get; private set; } = new List<Comment>();

        private User() { } // EF Core

        public static User Create(string firstName, string lastName, string email, string? bio = null)
        {
            Guard.Against.NullOrEmpty(firstName, nameof(firstName), "First name cannot be null or empty.");
            Guard.Against.NullOrEmpty(lastName, nameof(lastName), "Last name cannot be null or empty.");
            Guard.Against.NullOrEmpty(email, nameof(email), "Email cannot be null or empty.");

            return new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = Email.Create(email),
                Bio = bio
            };
        }

        public void UpdateProfile(string firstName, string lastName, string? bio, string? profilePictureUrl)
        {
            FirstName = firstName;
            LastName = lastName;
            Bio = bio;
            ProfilePictureUrl = profilePictureUrl;

            LastModifiedDate = DateTime.Now;
        }

        public void Deactivate()
        {
            IsActive = false;

            LastModifiedDate = DateTime.Now;
        }

        public string FullName => $"{FirstName} {LastName}";
    }
}