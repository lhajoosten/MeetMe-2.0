using AutoMapper;
using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Meetings.DTOs;
using MeetMe.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MeetMe.Application.Features.Meetings.Queries.GetMeeting
{
    public class GetMeetingByIdQueryHandler : IQueryHandler<GetMeetingByIdQuery, MeetingDetailDto>
    {
        private readonly IQueryRepository<Meeting, int> _meetingRepository;
        private readonly IMapper _mapper;

        public GetMeetingByIdQueryHandler(IQueryRepository<Meeting, int> meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        public async Task<Result<MeetingDetailDto>> Handle(GetMeetingByIdQuery request, CancellationToken cancellationToken)
        {
            // Use a more explicit approach to load related data
            var meeting = await _meetingRepository.AsQueryable()
                .Include(m => m.Creator)
                .Include(m => m.Attendees)
                    .ThenInclude(a => a.User)
                .Include(m => m.Posts)
                    .ThenInclude(p => p.Author)
                .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

            if (meeting == null)
            {
                return Result.Failure<MeetingDetailDto>("Meeting not found");
            }

            var meetingDetailDto = _mapper.Map<MeetingDetailDto>(meeting);
            return Result.Success(meetingDetailDto);
        }
    }
}
