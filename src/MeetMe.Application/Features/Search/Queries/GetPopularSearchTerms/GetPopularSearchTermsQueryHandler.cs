using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MediatR;

namespace MeetMe.Application.Features.Search.Queries.GetPopularSearchTerms;

public class GetPopularSearchTermsQueryHandler : IRequestHandler<GetPopularSearchTermsQuery, Result<List<string>>>
{
    private readonly ISearchService _searchService;

    public GetPopularSearchTermsQueryHandler(ISearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<Result<List<string>>> Handle(GetPopularSearchTermsQuery request, CancellationToken cancellationToken)
    {
        return await _searchService.GetPopularSearchTermsAsync(request.Count, cancellationToken);
    }
}
