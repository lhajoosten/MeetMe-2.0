using MeetMe.Application.Common.Models;

namespace MeetMe.Application.Common.Interfaces;

/// <summary>
/// Interface for search functionality across different content types
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Performs a global search across all content types
    /// </summary>
    Task<SearchResultsDto> GlobalSearchAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches specifically for meetings
    /// </summary>
    Task<Result<List<MeetingSearchResultDto>>> SearchMeetingsAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches specifically for posts
    /// </summary>
    Task<Result<List<PostSearchResultDto>>> SearchPostsAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches specifically for comments
    /// </summary>
    Task<Result<List<CommentSearchResultDto>>> SearchCommentsAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches specifically for users
    /// </summary>
    Task<Result<List<UserSearchResultDto>>> SearchUsersAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets search suggestions for autocomplete
    /// </summary>
    Task<Result<List<SearchSuggestionDto>>> GetSearchSuggestionsAsync(
        string query,
        int maxSuggestions = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets popular search terms
    /// </summary>
    Task<Result<List<string>>> GetPopularSearchTermsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);
}
