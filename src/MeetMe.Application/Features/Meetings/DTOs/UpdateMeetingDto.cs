namespace MeetMe.Application.Features.Meetings.DTOs
{
    /// <summary>
    /// DTO for updating an existing meeting
    /// </summary>
    public record UpdateMeetingDto
    {
        public int Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Location { get; init; } = string.Empty;
        public DateTime StartDateTime { get; init; }
        public DateTime EndDateTime { get; init; }
        public int? MaxAttendees { get; init; }
        public bool IsPublic { get; init; }
    }
}
