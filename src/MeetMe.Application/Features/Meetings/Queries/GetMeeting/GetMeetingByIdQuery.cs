using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Features.Meetings.DTOs;

namespace MeetMe.Application.Features.Meetings.Queries.GetMeeting
{
    public record GetMeetingByIdQuery : IQuery<MeetingDetailDto>
    {
        public int Id { get; init; }

        public GetMeetingByIdQuery(int id)
        {
            Id = id;
        }
    }
}
