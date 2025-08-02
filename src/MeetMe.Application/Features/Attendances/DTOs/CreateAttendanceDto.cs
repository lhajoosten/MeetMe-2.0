namespace MeetMe.Application.Features.Attendances.DTOs
{
    /// <summary>
    /// DTO for creating a new attendance record
    /// </summary>
    public record CreateAttendanceDto
    {
        public int UserId { get; init; }
        public int MeetingId { get; init; }
        public string Status { get; init; } = "Confirmed";
    }
}
