using MediatR;
using MeetMe.Application.Features.Search.Queries.GlobalSearch;
using MeetMe.Application.Features.Search.Queries.SearchMeetings;
using MeetMe.Application.Features.Search.Queries.SearchPosts;
using MeetMe.Application.Features.Search.Queries.SearchUsers;
using MeetMe.Application.Features.Search.Queries.GetSearchSuggestions;
using MeetMe.Application.Features.Search.Queries.GetPopularSearchTerms;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MeetMe.Application.Features.Search.DTOs;

namespace MeetMe.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Performs a global search across all content types
    /// </summary>
    [HttpGet("global")]
    public async Task<ActionResult<SearchResultsDto>> GlobalSearch(
        [FromQuery] string query,
        [FromQuery] string? types = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] bool activeOnly = true,
        [FromQuery] string sortBy = "Relevance",
        [FromQuery] string sortDirection = "Desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filters = new SearchFilters
        {
            Types = string.IsNullOrEmpty(types) ? new List<string>() : types.Split(',').ToList(),
            FromDate = fromDate,
            ToDate = toDate,
            ActiveOnly = activeOnly,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var searchQuery = new GlobalSearchQuery(query, filters, page, pageSize);
        var result = await _mediator.Send(searchQuery);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Searches specifically for meetings
    /// </summary>
    [HttpGet("meetings")]
    public async Task<ActionResult<List<MeetingSearchResultDto>>> SearchMeetings(
        [FromQuery] string query,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] bool activeOnly = true,
        [FromQuery] string sortBy = "Date",
        [FromQuery] string sortDirection = "Desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filters = new SearchFilters
        {
            FromDate = fromDate,
            ToDate = toDate,
            ActiveOnly = activeOnly,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var searchQuery = new SearchMeetingsQuery(query, filters, page, pageSize);
        var result = await _mediator.Send(searchQuery);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Searches specifically for posts
    /// </summary>
    [HttpGet("posts")]
    public async Task<ActionResult<List<PostSearchResultDto>>> SearchPosts(
        [FromQuery] string query,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] bool activeOnly = true,
        [FromQuery] string sortBy = "Date",
        [FromQuery] string sortDirection = "Desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filters = new SearchFilters
        {
            FromDate = fromDate,
            ToDate = toDate,
            ActiveOnly = activeOnly,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var searchQuery = new SearchPostsQuery(query, filters, page, pageSize);
        var result = await _mediator.Send(searchQuery);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Searches specifically for users
    /// </summary>
    [HttpGet("users")]
    public async Task<ActionResult<List<UserSearchResultDto>>> SearchUsers(
        [FromQuery] string query,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] bool activeOnly = true,
        [FromQuery] string sortBy = "Title",
        [FromQuery] string sortDirection = "Asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var filters = new SearchFilters
        {
            FromDate = fromDate,
            ToDate = toDate,
            ActiveOnly = activeOnly,
            SortBy = sortBy,
            SortDirection = sortDirection
        };

        var searchQuery = new SearchUsersQuery(query, filters, page, pageSize);
        var result = await _mediator.Send(searchQuery);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets search suggestions for autocomplete
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<SearchSuggestionDto>>> GetSearchSuggestions(
        [FromQuery] string query,
        [FromQuery] int maxSuggestions = 10)
    {
        var searchQuery = new GetSearchSuggestionsQuery(query, maxSuggestions);
        var result = await _mediator.Send(searchQuery);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets popular search terms
    /// </summary>
    [HttpGet("popular-terms")]
    public async Task<ActionResult<List<string>>> GetPopularSearchTerms([FromQuery] int count = 10)
    {
        var query = new GetPopularSearchTermsQuery(count);
        var result = await _mediator.Send(query);

        if (result.IsFailure)
        {
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}
