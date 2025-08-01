using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Features.Search.Queries.SearchUsers;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, Result<List<UserSearchResultDto>>>
{
    private readonly ISearchService _searchService;

    public SearchUsersQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<Result<List<UserSearchResultDto>>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _searchService.SearchUsersAsync(
                request.Query,
                request.Filters,
                request.Page,
                request.PageSize,
                cancellationToken);

            return result;
        }
        catch (Exception ex)
        {
            return Result.Failure<List<UserSearchResultDto>>($"User search failed: {ex.Message}");
        }
    }
}
