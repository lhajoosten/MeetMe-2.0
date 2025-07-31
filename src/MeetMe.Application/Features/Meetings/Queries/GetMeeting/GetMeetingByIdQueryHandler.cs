using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Meetings.Queries.GetMeeting
{
    public class GetMeetingByIdQueryHandler : IQueryHandler<GetMeetingByIdQuery, MeetingDto>
    {
        private readonly IQueryRepository<Meeting, Guid> _meetingRepository;

        public GetMeetingByIdQueryHandler(IQueryRepository<Meeting, Guid> meetingRepository)
        {
            _meetingRepository = meetingRepository;
        }

        public async Task<Result<MeetingDto>> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
        {
            var meeting = await _meetingRepository.GetByIdAsync(
                request.Id, 
                cancellationToken,
                m => m.Creator,
                m => m.Attendees,
                m => m.Posts);

            if (meeting == null)
            {
                return Result.Failure<MeetingDto>("Meeting not found");
            }

            var meetingDto = new MeetingDto
            {
                Id = meeting.Id,
                Title = meeting.Title,
                Description = meeting.Description,
                Location = meeting.Location.Value,
                StartDateTime = meeting.MeetingDateTime.StartDateTime,
                EndDateTime = meeting.MeetingDateTime.EndDateTime,
                MaxAttendees = meeting.MaxAttendees,
                IsActive = meeting.IsActive,
                CreatorId = meeting.CreatorId,
                CreatorName = meeting.Creator.FullName,
                AttendeeCount = meeting.Attendees.Count(a => a.IsActive),
                PostCount = meeting.Posts.Count(p => p.IsActive),
                IsUpcoming = meeting.IsUpcoming(),
                CreatedDate = meeting.CreatedDate
            };

            return Result.Success(meetingDto);
        }
    }
}
