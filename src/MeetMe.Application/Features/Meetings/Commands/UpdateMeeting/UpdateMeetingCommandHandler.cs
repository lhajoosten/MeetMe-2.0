using Ardalis.GuardClauses;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Application.Features.Meetings.Commands.UpdateMeeting;

public class UpdateMeetingCommandHandler : IRequestHandler<UpdateMeetingCommand, Result<Unit>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IQueryRepository<Meeting, Guid> _meetingQueryRepository;
    private readonly IQueryRepository<User, Guid> _userQueryRepository;

    public UpdateMeetingCommandHandler(
        IUnitOfWork unitOfWork, 
        IQueryRepository<Meeting, Guid> meetingQueryRepository,
        IQueryRepository<User, Guid> userQueryRepository)
    {
        _unitOfWork = unitOfWork;
        _meetingQueryRepository = meetingQueryRepository;
        _userQueryRepository = userQueryRepository;
    }

    public async Task<Result<Unit>> Handle(UpdateMeetingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var meeting = await _meetingQueryRepository.GetByIdAsync(request.Id, cancellationToken);
            
            if (meeting == null)
            {
                return Result.Failure<Unit>("Meeting not found");
            }

            var organizer = await _userQueryRepository.GetByIdAsync(request.OrganizerId, cancellationToken);
            
            if (organizer == null)
            {
                return Result.Failure<Unit>("Organizer not found");
            }

            var meetingDateTime = MeetingDateTime.Create(request.StartDateTime, request.EndDateTime);
            var location = Location.Create(request.Location);

            meeting.UpdateDetails(
                request.Title,
                request.Description,
                request.Location,
                request.StartDateTime,
                request.EndDateTime,
                organizer);

            var meetingRepository = _unitOfWork.CommandRepository<Meeting, Guid>();
            await meetingRepository.UpdateAsync(meeting, request.OrganizerId.ToString(), cancellationToken);
            await _unitOfWork.SaveChangesAsync(request.OrganizerId.ToString(), cancellationToken);

            return Result.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result.Failure<Unit>($"Failed to update meeting: {ex.Message}");
        }
    }
}
