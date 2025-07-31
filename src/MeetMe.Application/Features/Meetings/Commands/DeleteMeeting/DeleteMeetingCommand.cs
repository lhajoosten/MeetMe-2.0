using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Meetings.Commands.DeleteMeeting;

public record DeleteMeetingCommand : IRequest<Result<Unit>>
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
}
