using MeetMe.Application.Common.Models;
using MediatR;
using MeetMe.Application.Features.Search.DTOs;

namespace MeetMe.Application.Features.Search.Queries.GetSearchSuggestions;

public record GetSearchSuggestionsQuery(
    string Query,
    int MaxSuggestions = 10
) : IRequest<Result<List<SearchSuggestionDto>>>;
