using Ardalis.GuardClauses;
using MeetMe.Domain.Common;
using MeetMe.Domain.Events;

namespace MeetMe.Domain.Entities
{
    public class Attendance : BaseEntity
    {
        public Guid UserId { get; private set; }
        public Guid MeetingId { get; private set; }
        public AttendanceStatus Status { get; private set; }
        public DateTime JoinedAt { get; private set; }
        public bool IsActive { get; private set; } = true;

        public User User { get; private set; } = null!;
        public Meeting Meeting { get; private set; } = null!;

        private Attendance() { } // EF Core

        public static Attendance Create(User user, Meeting meeting)
        {
            Guard.Against.Null(user, nameof(user), "User cannot be null when creating attendance.");
            Guard.Against.Null(meeting, nameof(meeting), "Meeting cannot be null when creating attendance.");

            var attendance = new Attendance
            {
                User = user,
                Meeting = meeting,
                Status = AttendanceStatus.Confirmed,
                JoinedAt = DateTime.Now
            };

            attendance.AddDomainEvent(new UserJoinedMeetingEvent(attendance));

            return attendance;
        }

        public void ChangeStatus(AttendanceStatus status, User user)
        {
            Guard.Against.Null(status, nameof(status), "Status cannot be null when changing attendance status.");
            Guard.Against.Null(user, nameof(user), "User cannot be null when changing attendance status.");

            Status = status;
            LastModifiedDate = DateTime.Now;
            LastModifiedByUserId = user.Id;
        }

        public void Leave(User user)
        {
            IsActive = false;
            LastModifiedDate = DateTime.Now;
            LastModifiedByUserId = user.Id;

            AddDomainEvent(new UserLeftMeetingEvent(this));
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