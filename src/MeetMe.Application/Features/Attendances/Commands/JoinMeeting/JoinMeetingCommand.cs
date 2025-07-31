using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Attendances.Commands.JoinMeeting;

public record JoinMeetingCommand : IRequest<Result<Guid>>
{
    public Guid MeetingId { get; init; }
    public Guid UserId { get; init; }
}
