using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Meetings.Queries.GetAllMeetings
{
    public record GetMeetingsQuery : IQuery<List<MeetingDto>>
    {
        public bool? IsUpcoming { get; init; }
        public bool? IsActive { get; init; } = true;
        public Guid? CreatorId { get; init; }
        public string? SearchTerm { get; init; }
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 10;
    }
}
