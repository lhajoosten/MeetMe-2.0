using MeetMe.Application.Common.Models;
using MediatR;

namespace MeetMe.Application.Features.Search.Queries.GetSearchSuggestions;

public record GetSearchSuggestionsQuery(
    string Query,
    int MaxSuggestions = 10
) : IRequest<Result<List<SearchSuggestionDto>>>;
