using Ardalis.GuardClauses;
using MeetMe.Domain.Common;
using MeetMe.Domain.Events;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Domain.Entities
{
    public class Meeting : BaseEntity<int>
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public Location Location { get; private set; } = null!;
        public MeetingDateTime MeetingDateTime { get; private set; } = null!;
        public int? MaxAttendees { get; private set; }
        public bool IsPublic { get; private set; }
        public int CreatorId { get; private set; }

        public User Creator { get; private set; } = null!;
        public ICollection<Attendance> Attendees { get; private set; } = new List<Attendance>();
        public ICollection<Post> Posts { get; private set; } = new List<Post>();

        private Meeting() { } // EF Core

        public static Meeting Create(
            string title,
            string description,
            string location,
            DateTime startDateTime,
            DateTime endDateTime,
            User creator,
            int? maxAttendees = null,
            bool isPublic = true)
        {
            Guard.Against.NullOrWhiteSpace(title, nameof(title), "Title cannot be null or empty.");
            Guard.Against.NullOrWhiteSpace(description, nameof(description), "Description cannot be null or empty.");
            Guard.Against.Null(creator, nameof(creator), "Creator cannot be null when creating a meeting.");

            var meeting = new Meeting
            {
                Title = title,
                Description = description,
                Location = Location.Create(location),
                MeetingDateTime = MeetingDateTime.Create(startDateTime, endDateTime),
                CreatorId = creator.Id,
                Creator = creator,
                MaxAttendees = maxAttendees,
                IsPublic = isPublic
            };

            // Domain event
            meeting.AddDomainEvent(new MeetingCreatedEvent(meeting));

            return meeting;
        }

        public void UpdateDetails(string title, string description, string location, DateTime startDateTime, DateTime endDateTime, User user)
        {
            Title = title;
            Description = description;
            Location = Location.Create(location);
            MeetingDateTime = MeetingDateTime.Create(startDateTime, endDateTime);

            LastModifiedDate = DateTime.Now;
            LastModifiedByUserId = user.Id;
        }

        public bool CanAcceptMoreAttendees()
        {
            return MaxAttendees == null || Attendees.Count(a => a.IsActive) < MaxAttendees;
        }

        public bool IsUpcoming()
        {
            return MeetingDateTime.StartDateTime > DateTime.UtcNow;
        }

        public void Cancel(User user)
        {
            IsActive = false;
            LastModifiedDate = DateTime.Now;
            LastModifiedByUserId = user.Id;
        }

        private readonly List<IDomainEvent> _domainEvents = new();
        public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void RemoveDomainEvent(IDomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}