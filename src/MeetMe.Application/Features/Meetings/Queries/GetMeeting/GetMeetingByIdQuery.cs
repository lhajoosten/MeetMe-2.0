using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Meetings.Queries.GetMeeting
{
    public record GetMeetingByIdQuery : IQuery<MeetingDto>
    {
        public Guid Id { get; init; }

        public GetMeetingByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}
