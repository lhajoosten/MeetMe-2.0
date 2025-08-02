using MediatR;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Application.Features.Meetings.Commands.UpdateMeeting;

public record UpdateMeetingCommand : IRequest<Result<Unit>>
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime StartDateTime { get; init; }
    public DateTime EndDateTime { get; init; }
    public string Location { get; init; } = string.Empty;
    public int MaxAttendees { get; init; }
    public int OrganizerId { get; init; }
}
