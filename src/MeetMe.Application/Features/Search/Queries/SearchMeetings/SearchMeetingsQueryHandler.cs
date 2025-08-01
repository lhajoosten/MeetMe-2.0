using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Search.Queries.SearchMeetings;

public class SearchMeetingsQueryHandler : IRequestHandler<SearchMeetingsQuery, Result<List<MeetingSearchResultDto>>>
{
    private readonly ISearchService _searchService;

    public SearchMeetingsQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<Result<List<MeetingSearchResultDto>>> Handle(SearchMeetingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _searchService.SearchMeetingsAsync(
                request.Query,
                request.Filters,
                request.Page,
                request.PageSize,
                cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<MeetingSearchResultDto>>($"Meeting search failed: {ex.Message}");
        }
    }
}
