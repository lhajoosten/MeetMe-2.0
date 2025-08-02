using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Meetings.Commands.CreateMeeting
{
    public class CreateMeetingCommandHandler : ICommandHandler<CreateMeetingCommand, int>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IQueryRepository<User, int> _userQueryRepository;

        public CreateMeetingCommandHandler(IUnitOfWork unitOfWork, IQueryRepository<User, int> userQueryRepository)
        {
            _unitOfWork = unitOfWork;
            _userQueryRepository = userQueryRepository;
        }

        public async Task<Result<int>> Handle(CreateMeetingCommand request, CancellationToken cancellationToken)
        {
            // Get the creator user
            var creator = await _userQueryRepository.GetByIdAsync(request.CreatorId, cancellationToken);
            if (creator == null)
            {
                return Result.Failure<int>("Creator not found");
            }

            // Create the meeting using domain factory method
            var meeting = Meeting.Create(
                request.Title,
                request.Description,
                request.Location,
                request.StartDateTime,
                request.EndDateTime,
                creator,
                request.MaxAttendees,
                request.IsPublic);

            // Add to repository
            var meetingRepository = _unitOfWork.CommandRepository<Meeting, int>();
            await meetingRepository.AddAsync(meeting, request.CreatorId, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(request.CreatorId, cancellationToken);

            return Result.Success(meeting.Id);
        }
    }
}
