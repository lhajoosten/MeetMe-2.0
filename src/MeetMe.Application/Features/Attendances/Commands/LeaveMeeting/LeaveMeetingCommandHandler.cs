using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Attendances.Commands.LeaveMeeting;

public class LeaveMeetingCommandHandler : IRequestHandler<LeaveMeetingCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryRepository<User, Guid> _userQueryRepository;
    private readonly IQueryRepository<Attendance, int> _attendanceQueryRepository;

    public LeaveMeetingCommandHandler(
        IUnitOfWork unitOfWork,
        IQueryRepository<User, Guid> userQueryRepository,
        IQueryRepository<Attendance, int> attendanceQueryRepository)
    {
        _unitOfWork = unitOfWork;
        _userQueryRepository = userQueryRepository;
        _attendanceQueryRepository = attendanceQueryRepository;
    }

    public async Task<Result<Unit>> Handle(LeaveMeetingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the user
            var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<Unit>("User not found");
            }

            // Find the attendance record
            var attendance = await _attendanceQueryRepository
                .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.MeetingId == request.MeetingId && a.IsActive, cancellationToken);

            if (attendance == null)
            {
                return Result.Failure<Unit>("User is not attending this meeting");
            }

            // Leave the meeting
            attendance.Leave(user);

            // Update in repository
            var attendanceRepository = _unitOfWork.CommandRepository<Attendance, int>();
            await attendanceRepository.UpdateAsync(attendance, request.UserId.ToString(), cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(request.UserId.ToString(), cancellationToken);

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<Unit>($"Failed to leave meeting: {ex.Message}");
        }
    }
}
