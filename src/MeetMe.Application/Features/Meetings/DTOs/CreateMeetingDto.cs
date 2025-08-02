namespace MeetMe.Application.Features.Meetings.DTOs
{
    /// <summary>
    /// DTO for creating a new meeting
    /// </summary>
    public record CreateMeetingDto
    {
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public int? MaxAttendees { get; init; }
        public bool IsPublic { get; init; } = true;
    }
}
