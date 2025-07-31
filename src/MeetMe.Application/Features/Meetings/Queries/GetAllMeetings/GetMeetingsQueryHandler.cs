using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;
using System.Linq.Expressions;

namespace MeetMe.Application.Features.Meetings.Queries.GetAllMeetings
{
    public class GetMeetingsQueryHandler : IQueryHandler<GetMeetingsQuery, List<MeetingDto>>
    {
        private readonly IQueryRepository<Meeting, Guid> _meetingRepository;

        public GetMeetingsQueryHandler(IQueryRepository<Meeting, Guid> meetingRepository)
        {
            _meetingRepository = meetingRepository;
        }

        public async Task<Result<List<MeetingDto>>> Handle(GetMeetingsQuery request, CancellationToken cancellationToken)
        {
            // Build the filter expression
            Expression<Func<Meeting, bool>> filter = m => true;

            if (request.IsActive.HasValue)
            {
                filter = CombineExpressions(filter, m => m.IsActive == request.IsActive.Value);
            }

            if (request.CreatorId.HasValue)
            {
                filter = CombineExpressions(filter, m => m.CreatorId == request.CreatorId.Value);
            }

            if (request.IsUpcoming.HasValue)
            {
                if (request.IsUpcoming.Value)
                {
                    filter = CombineExpressions(filter, m => m.MeetingDateTime.StartDateTime > DateTime.UtcNow);
                }
                else
                {
                    filter = CombineExpressions(filter, m => m.MeetingDateTime.StartDateTime <= DateTime.UtcNow);
                }
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                filter = CombineExpressions(filter, m => 
                    m.Title.ToLower().Contains(searchTerm) || 
                    m.Description.ToLower().Contains(searchTerm) ||
                    m.Location.Value.ToLower().Contains(searchTerm));
            }

            // Get meetings with related data
            var meetings = await _meetingRepository.FindAsync(
                filter, 
                cancellationToken,
                m => m.Creator,
                m => m.Attendees,
                m => m.Posts);

            // Project to DTOs
            var meetingDtos = meetings
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(m => new MeetingDto
                {
                    Id = m.Id,
                    Title = m.Title,
                    Description = m.Description,
                    Location = m.Location.Value,
                    StartDateTime = m.MeetingDateTime.StartDateTime,
                    EndDateTime = m.MeetingDateTime.EndDateTime,
                    MaxAttendees = m.MaxAttendees,
                    IsActive = m.IsActive,
                    CreatorId = m.CreatorId,
                    CreatorName = m.Creator.FullName,
                    AttendeeCount = m.Attendees.Count(a => a.IsActive),
                    PostCount = m.Posts.Count(p => p.IsActive),
                    IsUpcoming = m.IsUpcoming(),
                    CreatedDate = m.CreatedDate
                })
                .OrderBy(m => m.StartDateTime)
                .ToList();

            return Result.Success(meetingDtos);
        }

        private static Expression<Func<Meeting, bool>> CombineExpressions(
            Expression<Func<Meeting, bool>> expr1,
            Expression<Func<Meeting, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(Meeting));
            var left = new ParameterReplacer(parameter).Visit(expr1.Body);
            var right = new ParameterReplacer(parameter).Visit(expr2.Body);
            var combined = Expression.AndAlso(left!, right!);
            return Expression.Lambda<Func<Meeting, bool>>(combined, parameter);
        }
    }

    public class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;

        public ParameterReplacer(ParameterExpression parameter)
        {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(_parameter);
        }
    }
}
