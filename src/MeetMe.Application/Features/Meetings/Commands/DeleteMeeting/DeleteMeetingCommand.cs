using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Meetings.Commands.DeleteMeeting;

public record DeleteMeetingCommand : IRequest<Result<Unit>>
{
    public int Id { get; init; }
    public int UserId { get; init; }
}
