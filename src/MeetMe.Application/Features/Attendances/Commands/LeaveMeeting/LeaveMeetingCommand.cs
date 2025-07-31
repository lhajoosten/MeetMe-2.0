using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Attendances.Commands.LeaveMeeting;

public record LeaveMeetingCommand : IRequest<Result<Unit>>
{
    public Guid MeetingId { get; init; }
    public Guid UserId { get; init; }
}
