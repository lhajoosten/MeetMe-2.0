using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Meetings.Commands.DeleteMeeting;

public class DeleteMeetingCommandHandler : IRequestHandler<DeleteMeetingCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryRepository<Meeting, int> _meetingQueryRepository;
    private readonly IQueryRepository<User, int> _userQueryRepository;

    public DeleteMeetingCommandHandler(
        IUnitOfWork unitOfWork,
        IQueryRepository<Meeting, int> meetingQueryRepository,
        IQueryRepository<User, int> userQueryRepository)
    {
        _unitOfWork = unitOfWork;
        _meetingQueryRepository = meetingQueryRepository;
        _userQueryRepository = userQueryRepository;
    }

    public async Task<Result<Unit>> Handle(DeleteMeetingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _meetingQueryRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (meeting == null)
            {
                return Result.Failure<Unit>("Meeting not found");
            }

            var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
            
            if (user == null)
            {
                return Result.Failure<Unit>("User not found");
            }

            // Check if the user is the creator of the meeting
            if (meeting.CreatorId != request.UserId)
            {
                return Result.Failure<Unit>("Only the meeting creator can delete the meeting");
            }

            var meetingRepository = _unitOfWork.CommandRepository<Meeting, int>();
            await meetingRepository.SoftDeleteAsync(meeting, request.UserId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(request.UserId, cancellationToken);

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<Unit>($"Failed to delete meeting: {ex.Message}");
        }
    }
}
