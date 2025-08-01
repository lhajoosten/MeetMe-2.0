using MeetMe.Application.Common.Models;
using MediatR;

namespace MeetMe.Application.Features.Search.Queries.SearchMeetings;

public record SearchMeetingsQuery(
    string Query,
    SearchFilters? Filters = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<List<MeetingSearchResultDto>>>;
