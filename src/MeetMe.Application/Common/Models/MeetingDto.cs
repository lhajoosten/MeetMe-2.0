namespace MeetMe.Application.Common.Models
{
    public record MeetingDto
    {
        public Guid Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public DateTime StartDateTime { get; init; }
        public DateTime EndDateTime { get; init; }
        public int? MaxAttendees { get; init; }
        public bool IsActive { get; init; }
        public Guid CreatorId { get; init; }
        public string CreatorName { get; init; } = string.Empty;
        public int AttendeeCount { get; init; }
        public int PostCount { get; init; }
        public bool IsUpcoming { get; init; }
        public DateTime CreatedDate { get; init; }
    }
}
