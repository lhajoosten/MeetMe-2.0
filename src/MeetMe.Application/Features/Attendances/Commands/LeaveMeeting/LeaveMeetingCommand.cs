using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Attendances.Commands.LeaveMeeting;

public record LeaveMeetingCommand : IRequest<Result<Unit>>
{
    public int MeetingId { get; init; }
    public int UserId { get; init; }
}
