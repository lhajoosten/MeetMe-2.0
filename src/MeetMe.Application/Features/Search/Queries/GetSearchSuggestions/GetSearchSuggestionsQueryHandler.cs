using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Search.Queries.GetSearchSuggestions;

public class GetSearchSuggestionsQueryHandler : IRequestHandler<GetSearchSuggestionsQuery, Result<List<SearchSuggestionDto>>>
{
    private readonly ISearchService _searchService;

    public GetSearchSuggestionsQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<Result<List<SearchSuggestionDto>>> Handle(GetSearchSuggestionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _searchService.GetSearchSuggestionsAsync(
                request.Query,
                request.MaxSuggestions,
                cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SearchSuggestionDto>>($"Failed to get search suggestions: {ex.Message}");
        }
    }
}
