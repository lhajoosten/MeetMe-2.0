using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using System.Diagnostics;

namespace MeetMe.Application.Features.Search.Queries.GlobalSearch;

public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, Result<SearchResultsDto>>
{
    private readonly ISearchService _searchService;

    public GlobalSearchQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<Result<SearchResultsDto>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var results = await _searchService.GlobalSearchAsync(
                request.Query,
                request.Filters,
                request.Page,
                request.PageSize,
                cancellationToken);

            stopwatch.Stop();
            results.SearchDuration = stopwatch.Elapsed;

            return Result.Success(results);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return Result.Failure<SearchResultsDto>($"Search failed: {ex.Message}");
        }
    }
}
