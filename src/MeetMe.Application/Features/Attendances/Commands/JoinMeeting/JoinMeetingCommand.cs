using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Attendances.Commands.JoinMeeting;

public record JoinMeetingCommand : IRequest<Result<int>>
{
    public int MeetingId { get; init; }
    public int UserId { get; init; }
}
