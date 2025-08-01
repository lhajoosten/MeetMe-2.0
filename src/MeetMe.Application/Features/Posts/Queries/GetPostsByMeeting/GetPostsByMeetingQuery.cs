using MediatR;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Posts.Queries.GetPostsByMeeting;

public record GetPostsByMeetingQuery : IRequest<Result<List<PostDto>>>
{
    public Guid MeetingId { get; init; }

    public GetPostsByMeetingQuery(Guid meetingId)
    {
        MeetingId = meetingId;
    }
}
