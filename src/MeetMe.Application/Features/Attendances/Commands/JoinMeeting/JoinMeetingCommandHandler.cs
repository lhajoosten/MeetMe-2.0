using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;

namespace MeetMe.Application.Features.Attendances.Commands.JoinMeeting;

public class JoinMeetingCommandHandler : IRequestHandler<JoinMeetingCommand, Result<int>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryRepository<Meeting, int> _meetingQueryRepository;
    private readonly IQueryRepository<User, int> _userQueryRepository;
    private readonly IQueryRepository<Attendance, int> _attendanceQueryRepository;

    public JoinMeetingCommandHandler(
        IUnitOfWork unitOfWork,
        IQueryRepository<Meeting, int> meetingQueryRepository,
        IQueryRepository<User, int> userQueryRepository,
        IQueryRepository<Attendance, int> attendanceQueryRepository)
    {
        _unitOfWork = unitOfWork;
        _meetingQueryRepository = meetingQueryRepository;
        _userQueryRepository = userQueryRepository;
        _attendanceQueryRepository = attendanceQueryRepository;
    }

    public async Task<Result<int>> Handle(JoinMeetingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the meeting
            var meeting = await _meetingQueryRepository.GetByIdAsync(request.MeetingId, cancellationToken);
            if (meeting == null)
            {
                return Result.Failure<int>("Meeting not found");
            }

            // Get the user
            var user = await _userQueryRepository.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result.Failure<int>("User not found");
            }

            // Check if user is already attending this meeting
            var existingAttendance = await _attendanceQueryRepository
                .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.MeetingId == request.MeetingId, cancellationToken);

            if (existingAttendance != null && existingAttendance.IsActive)
            {
                return Result.Failure<int>("User is already attending this meeting");
            }

            // Check if meeting has reached maximum capacity
            var currentAttendeeCount = await _attendanceQueryRepository
                .CountAsync(a => a.MeetingId == request.MeetingId && a.IsActive, cancellationToken);

            if (meeting.MaxAttendees.HasValue && currentAttendeeCount >= meeting.MaxAttendees.Value)
            {
                return Result.Failure<int>("Meeting has reached maximum capacity");
            }

            // Create attendance
            var attendance = Attendance.Create(user, meeting);

            // Add to repository
            var attendanceRepository = _unitOfWork.CommandRepository<Attendance, int>();
            await attendanceRepository.AddAsync(attendance, request.UserId, cancellationToken);

            // Save changes
            await _unitOfWork.SaveChangesAsync(request.UserId, cancellationToken);

            return Result.Success(meeting.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<int>($"Failed to join meeting: {ex.Message}");
        }
    }
}
