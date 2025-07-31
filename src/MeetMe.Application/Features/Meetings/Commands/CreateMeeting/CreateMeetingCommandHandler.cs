using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Meetings.Commands.CreateMeeting
{
    public class CreateMeetingCommandHandler : ICommandHandler<CreateMeetingCommand, Guid>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQueryRepository<User, Guid> _userQueryRepository;

        public CreateMeetingCommandHandler(IUnitOfWork unitOfWork, IQueryRepository<User, Guid> userQueryRepository)
        {
            _unitOfWork = unitOfWork;
            _userQueryRepository = userQueryRepository;
        }

        public async Task<Result<Guid>> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
        {
            // Get the creator user
            var creator = await _userQueryRepository.GetByIdAsync(request.CreatorId, cancellationToken);
            if (creator == null)
            {
                return Result.Failure<Guid>("Creator not found");
            }

            // Create the meeting using domain factory method
            var meeting = Meeting.Create(
                request.Title,
                request.Description,
                request.Location,
                request.StartDateTime,
                request.EndDateTime,
                creator,
                request.MaxAttendees);

            // Add to repository
            var meetingRepository = _unitOfWork.CommandRepository<Meeting, Guid>();
            await meetingRepository.AddAsync(meeting, request.CreatorId.ToString(), cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(request.CreatorId.ToString(), cancellationToken);

            return Result.Success(meeting.Id);
        }
    }
}
