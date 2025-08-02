using MeetMe.Application.Common.Models;
using MediatR;
using MeetMe.Application.Features.Search.DTOs;

namespace MeetMe.Application.Features.Search.Queries.GlobalSearch;

public record GlobalSearchQuery(
    string Query,
    SearchFilters? Filters = null,
    int Page = 1,
    int PageSize = 20
) : IRequest<Result<SearchResultsDto>>;
