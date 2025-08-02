using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Features.Posts.DTOs;

namespace MeetMe.Application.Features.Posts.Queries.GetPostsByMeeting;

public record GetPostsByMeetingQuery : IQuery<List<PostDetailDto>>
{
    public int MeetingId { get; init; }

    public GetPostsByMeetingQuery(int meetingId)
    {
        MeetingId = meetingId;
    }
}
