namespace MeetMe.Application.Features.Attendances.DTOs
{
    /// <summary>
    /// DTO for updating attendance status
    /// </summary>
    public record UpdateAttendanceDto
    {
        public int Id { get; init; }
        public string Status { get; init; } = string.Empty;
    }
}
