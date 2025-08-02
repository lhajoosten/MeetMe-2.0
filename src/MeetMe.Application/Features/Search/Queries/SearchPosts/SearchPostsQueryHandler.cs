using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Search.DTOs;

namespace MeetMe.Application.Features.Search.Queries.SearchPosts;

public class SearchPostsQueryHandler : IRequestHandler<SearchPostsQuery, Result<List<PostSearchResultDto>>>
{
    private readonly ISearchService _searchService;

    public SearchPostsQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<Result<List<PostSearchResultDto>>> Handle(SearchPostsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _searchService.SearchPostsAsync(
                request.Query,
                request.Filters,
                request.Page,
                request.PageSize,
                cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<PostSearchResultDto>>($"Post search failed: {ex.Message}");
        }
    }
}
