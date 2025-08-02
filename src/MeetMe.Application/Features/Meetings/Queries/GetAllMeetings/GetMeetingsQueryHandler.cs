using MeetMe.Application.Common.Abstraction;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Meetings.DTOs;
using MeetMe.Domain.Entities;
using System.Linq.Expressions;
using AutoMapper;

namespace MeetMe.Application.Features.Meetings.Queries.GetAllMeetings
{
    public class GetMeetingsQueryHandler : IQueryHandler<GetMeetingsQuery, List<MeetingSummaryDto>>
    {
        private readonly IQueryRepository<Meeting, int> _meetingRepository;
        private readonly IMapper _mapper;

        public GetMeetingsQueryHandler(IQueryRepository<Meeting, int> meetingRepository, IMapper mapper)
        {
            _meetingRepository = meetingRepository;
            _mapper = mapper;
        }

        public async Task<Result<List<MeetingSummaryDto>>> Handle(GetMeetingsQuery request, CancellationToken cancellationToken)
        {
            // Build the filter expression
            Expression<Func<Meeting, bool>> filter = m => true;

            if (request.IsActive.HasValue)
            {
                filter = CombineExpressions(filter, m => m.IsActive == request.IsActive.Value);
            }

            if (request.IsPublic.HasValue)
            {
                filter = CombineExpressions(filter, m => m.IsPublic == request.IsPublic.Value);
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

            // Apply pagination and mapping
            var pagedMeetings = meetings
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .OrderBy(m => m.MeetingDateTime.StartDateTime)
                .ToList();

            var meetingSummaryDtos = _mapper.Map<List<MeetingSummaryDto>>(pagedMeetings);

            return Result.Success(meetingSummaryDtos);
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
