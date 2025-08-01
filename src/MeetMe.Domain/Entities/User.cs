using Ardalis.GuardClauses;
using MeetMe.Domain.Common;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Domain.Entities
{
    public class User : BaseEntity<Guid>
    {
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public Email Email { get; private set; } = null!;
        public string PasswordHash { get; private set; } = string.Empty;
        public string? Bio { get; private set; }
        public string? ProfilePictureUrl { get; private set; }
        public int RoleId { get; private set; }

        public MMIdentity? Identity { get; private set; }
        public Role? Role { get; private set; }

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
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = Email.Create(email),
                Bio = bio
            };
        }

        public static User Create(string firstName, string lastName, Email email, string passwordHash, string? bio = null)
        {
            Guard.Against.NullOrEmpty(firstName, nameof(firstName), "First name cannot be null or empty.");
            Guard.Against.NullOrEmpty(lastName, nameof(lastName), "Last name cannot be null or empty.");
            Guard.Against.Null(email, nameof(email), "Email cannot be null.");
            Guard.Against.NullOrEmpty(passwordHash, nameof(passwordHash), "Password hash cannot be null or empty.");

            return new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PasswordHash = passwordHash,
                Bio = bio
            };
        }

        public static User CreateFromIdentity(MMIdentity identity)
        {
            Guard.Against.Null(identity, nameof(identity), "Identity cannot be null when creating a user from identity.");
            
            return new User
            {
                Identity = identity,
                FirstName = identity.FirstName ?? string.Empty,
                LastName = identity.LastName ?? string.Empty,
                Email = Email.Create(identity.Email ?? string.Empty),
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

        public void UpdateEmail(string email)
        {
            Guard.Against.NullOrEmpty(email, nameof(email), "Email cannot be null or empty.");

            Email = Email.Create(email);
            LastModifiedDate = DateTime.Now;
        }

        public void SetPrimaryRole(Role role)
        {
            Guard.Against.Null(role, nameof(role), "Role cannot be null when setting primary role.");
         
            Role = role;
            RoleId = role.Id;
            LastModifiedDate = DateTime.Now;
        }

        public void SetIdentity(MMIdentity identity)
        {
            Guard.Against.Null(identity, nameof(identity), "Identity cannot be null when setting user identity.");
            
            if (Identity != null)
            {
                throw new InvalidOperationException("Identity is already set for this user.");
            }

            Identity = identity;
            LastModifiedDate = DateTime.Now;
        }

        public void Activate()
        {
            IsActive = true;
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