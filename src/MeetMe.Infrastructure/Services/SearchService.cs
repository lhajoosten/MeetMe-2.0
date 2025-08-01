using AutoMapper;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Domain.Entities;
using MeetMe.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace MeetMe.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public SearchService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<SearchResultsDto> GlobalSearchAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        var searchTerms = ParseSearchQuery(query);
        var results = new List<SearchResultDto>();
        var typeCounts = new Dictionary<string, int>();

        filters ??= new SearchFilters();

        // Search across all content types if no specific types are filtered
        var searchTypes = filters.Types.Any() ? filters.Types : new List<string> { "Meeting", "Post", "Comment", "User" };

        foreach (var type in searchTypes)
        {
            var typeResults = type switch
            {
                "Meeting" => await SearchMeetingsInternalAsync(searchTerms, filters, cancellationToken),
                "Post" => await SearchPostsInternalAsync(searchTerms, filters, cancellationToken),
                "Comment" => await SearchCommentsInternalAsync(searchTerms, filters, cancellationToken),
                "User" => await SearchUsersInternalAsync(searchTerms, filters, cancellationToken),
                _ => new List<SearchResultDto>()
            };

            results.AddRange(typeResults);
            typeCounts[type] = typeResults.Count;
        }

        // Apply sorting
        results = ApplySorting(results, filters.SortBy, filters.SortDirection);

        // Calculate pagination
        var totalCount = results.Count;
        var paginatedResults = results
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var searchDuration = DateTime.UtcNow - startTime;

        // Track the search query for analytics (fire and forget)
        _ = Task.Run(async () =>
        {
            try
            {
                await TrackSearchQueryAsync(query, "Global", null, totalCount, searchDuration, cancellationToken);
            }
            catch
            {
                // Ignore tracking errors - don't let them affect the search results
            }
        }, cancellationToken);

        return new SearchResultsDto
        {
            Results = paginatedResults,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Query = query,
            TypeCounts = typeCounts,
            SearchDuration = searchDuration
        };
    }

    public async Task<Result<List<MeetingSearchResultDto>>> SearchMeetingsAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        try
        {
            var searchTerms = ParseSearchQuery(query);
            filters ??= new SearchFilters();

            var meetingsQuery = _context.Meetings
                .Include(m => m.Creator)
                .Include(m => m.Attendees)
                .Where(m => filters.ActiveOnly ? m.IsActive : true);

            // Apply date filters
            if (filters.FromDate.HasValue)
                meetingsQuery = meetingsQuery.Where(m => m.MeetingDateTime.StartDateTime >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                meetingsQuery = meetingsQuery.Where(m => m.MeetingDateTime.StartDateTime <= filters.ToDate.Value);

            // Apply text search
            if (searchTerms.Any())
            {
                meetingsQuery = meetingsQuery.Where(m =>
                    searchTerms.Any(term =>
                        EF.Functions.Like(m.Title, $"%{term}%") ||
                        EF.Functions.Like(m.Description, $"%{term}%") ||
                        EF.Functions.Like(m.Location.Value, $"%{term}%")));
            }

            // Apply sorting
            meetingsQuery = filters.SortBy?.ToLower() switch
            {
                "date" => filters.SortDirection?.ToLower() == "asc" 
                    ? meetingsQuery.OrderBy(m => m.MeetingDateTime.StartDateTime)
                    : meetingsQuery.OrderByDescending(m => m.MeetingDateTime.StartDateTime),
                "title" => filters.SortDirection?.ToLower() == "asc"
                    ? meetingsQuery.OrderBy(m => m.Title)
                    : meetingsQuery.OrderByDescending(m => m.Title),
                _ => meetingsQuery.OrderByDescending(m => m.CreatedDate)
            };

            var meetings = await meetingsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var meetingDtos = meetings.Select(m => new MeetingSearchResultDto
            {
                Id = (int)m.Id.GetHashCode(), // Convert Guid to int for display
                Title = m.Title,
                Description = m.Description,
                StartDateTime = m.MeetingDateTime.StartDateTime,
                EndDateTime = m.MeetingDateTime.EndDateTime,
                Location = m.Location.Value,
                OrganizerName = $"{m.Creator.FirstName} {m.Creator.LastName}",
                AttendeeCount = m.Attendees.Count(a => a.Status == AttendanceStatus.Confirmed),
                IsActive = m.IsActive,
                CreatedDate = m.CreatedDate
            }).ToList();

            var searchDuration = DateTime.UtcNow - startTime;

            // Track the search query for analytics (fire and forget)
            _ = Task.Run(async () =>
            {
                try
                {
                    await TrackSearchQueryAsync(query, "Meeting", null, meetingDtos.Count, searchDuration, cancellationToken);
                }
                catch
                {
                    // Ignore tracking errors
                }
            }, cancellationToken);

            return Result.Success(meetingDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<MeetingSearchResultDto>>($"Meeting search failed: {ex.Message}");
        }
    }

    public async Task<Result<List<PostSearchResultDto>>> SearchPostsAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchTerms = ParseSearchQuery(query);
            filters ??= new SearchFilters();

            var postsQuery = _context.Posts
                .Include(p => p.Author)
                .Include(p => p.Meeting)
                .Include(p => p.Comments)
                .Where(p => filters.ActiveOnly ? p.IsActive : true);

            // Apply date filters
            if (filters.FromDate.HasValue)
                postsQuery = postsQuery.Where(p => p.CreatedDate >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                postsQuery = postsQuery.Where(p => p.CreatedDate <= filters.ToDate.Value);

            // Apply text search
            if (searchTerms.Any())
            {
                postsQuery = postsQuery.Where(p =>
                    searchTerms.Any(term =>
                        EF.Functions.Like(p.Title, $"%{term}%") ||
                        EF.Functions.Like(p.Content, $"%{term}%")));
            }

            // Apply sorting
            postsQuery = filters.SortBy?.ToLower() switch
            {
                "date" => filters.SortDirection?.ToLower() == "asc"
                    ? postsQuery.OrderBy(p => p.CreatedDate)
                    : postsQuery.OrderByDescending(p => p.CreatedDate),
                "title" => filters.SortDirection?.ToLower() == "asc"
                    ? postsQuery.OrderBy(p => p.Title)
                    : postsQuery.OrderByDescending(p => p.Title),
                _ => postsQuery.OrderByDescending(p => p.CreatedDate)
            };

            var posts = await postsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var postDtos = posts.Select(p => new PostSearchResultDto
            {
                Id = p.Id,
                Title = p.Title,
                Content = p.Content,
                AuthorName = $"{p.Author.FirstName} {p.Author.LastName}",
                MeetingId = (int)p.MeetingId.GetHashCode(), // Convert Guid to int for display
                MeetingTitle = p.Meeting.Title,
                CommentCount = p.Comments.Count(c => c.IsActive),
                IsActive = p.IsActive,
                CreatedDate = p.CreatedDate
            }).ToList();

            return Result.Success(postDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<PostSearchResultDto>>($"Post search failed: {ex.Message}");
        }
    }

    public async Task<Result<List<CommentSearchResultDto>>> SearchCommentsAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchTerms = ParseSearchQuery(query);
            filters ??= new SearchFilters();

            var commentsQuery = _context.Comments
                .Include(c => c.Author)
                .Include(c => c.Post)
                .Where(c => filters.ActiveOnly ? c.IsActive : true);

            // Apply date filters
            if (filters.FromDate.HasValue)
                commentsQuery = commentsQuery.Where(c => c.CreatedDate >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                commentsQuery = commentsQuery.Where(c => c.CreatedDate <= filters.ToDate.Value);

            // Apply text search
            if (searchTerms.Any())
            {
                commentsQuery = commentsQuery.Where(c =>
                    searchTerms.Any(term =>
                        EF.Functions.Like(c.Content, $"%{term}%")));
            }

            // Apply sorting
            commentsQuery = filters.SortBy?.ToLower() switch
            {
                "date" => filters.SortDirection?.ToLower() == "asc"
                    ? commentsQuery.OrderBy(c => c.CreatedDate)
                    : commentsQuery.OrderByDescending(c => c.CreatedDate),
                _ => commentsQuery.OrderByDescending(c => c.CreatedDate)
            };

            var comments = await commentsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var commentDtos = comments.Select(c => new CommentSearchResultDto
            {
                Id = c.Id,
                Content = c.Content,
                AuthorName = $"{c.Author.FirstName} {c.Author.LastName}",
                PostId = c.PostId,
                PostTitle = c.Post.Title,
                ParentCommentId = c.ParentCommentId,
                IsReply = c.ParentCommentId.HasValue,
                IsActive = c.IsActive,
                CreatedDate = c.CreatedDate
            }).ToList();

            return Result.Success(commentDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<CommentSearchResultDto>>($"Comment search failed: {ex.Message}");
        }
    }

    public async Task<Result<List<UserSearchResultDto>>> SearchUsersAsync(
        string query,
        SearchFilters? filters = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchTerms = ParseSearchQuery(query);
            filters ??= new SearchFilters();

            var usersQuery = _context.Users
                .Where(u => filters.ActiveOnly ? u.IsActive : true);

            // Apply date filters
            if (filters.FromDate.HasValue)
                usersQuery = usersQuery.Where(u => u.CreatedDate >= filters.FromDate.Value);

            if (filters.ToDate.HasValue)
                usersQuery = usersQuery.Where(u => u.CreatedDate <= filters.ToDate.Value);

            // Apply text search
            if (searchTerms.Any())
            {
                usersQuery = usersQuery.Where(u =>
                    searchTerms.Any(term =>
                        EF.Functions.Like(u.FirstName, $"%{term}%") ||
                        EF.Functions.Like(u.LastName, $"%{term}%") ||
                        EF.Functions.Like(u.Email.Value, $"%{term}%")));
            }

            // Apply sorting
            usersQuery = filters.SortBy?.ToLower() switch
            {
                "date" => filters.SortDirection?.ToLower() == "asc"
                    ? usersQuery.OrderBy(u => u.CreatedDate)
                    : usersQuery.OrderByDescending(u => u.CreatedDate),
                "title" => filters.SortDirection?.ToLower() == "asc"
                    ? usersQuery.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
                    : usersQuery.OrderByDescending(u => u.FirstName).ThenByDescending(u => u.LastName),
                _ => usersQuery.OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            };

            var users = await usersQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var userDtos = users.Select(u => new UserSearchResultDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email.Value,
                CreatedDate = u.CreatedDate,
                IsActive = u.IsActive
            }).ToList();

            return Result.Success(userDtos);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<UserSearchResultDto>>($"User search failed: {ex.Message}");
        }
    }

    public async Task<Result<List<SearchSuggestionDto>>> GetSearchSuggestionsAsync(
        string query,
        int maxSuggestions = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestions = new List<SearchSuggestionDto>();

            // Get meeting titles
            var meetingTitles = await _context.Meetings
                .Where(m => m.IsActive && EF.Functions.Like(m.Title, $"%{query}%"))
                .Select(m => m.Title)
                .Take(maxSuggestions / 4)
                .ToListAsync(cancellationToken);

            suggestions.AddRange(meetingTitles.Select(title => new SearchSuggestionDto
            {
                Text = title,
                Type = "Meeting",
                Count = 1
            }));

            // Get post titles
            var postTitles = await _context.Posts
                .Where(p => p.IsActive && EF.Functions.Like(p.Title, $"%{query}%"))
                .Select(p => p.Title)
                .Take(maxSuggestions / 4)
                .ToListAsync(cancellationToken);

            suggestions.AddRange(postTitles.Select(title => new SearchSuggestionDto
            {
                Text = title,
                Type = "Post",
                Count = 1
            }));

            // Get user names
            var userNames = await _context.Users
                .Where(u => u.IsActive && (
                    EF.Functions.Like(u.FirstName, $"%{query}%") ||
                    EF.Functions.Like(u.LastName, $"%{query}%")))
                .Select(u => $"{u.FirstName} {u.LastName}")
                .Take(maxSuggestions / 4)
                .ToListAsync(cancellationToken);

            suggestions.AddRange(userNames.Select(name => new SearchSuggestionDto
            {
                Text = name,
                Type = "User",
                Count = 1
            }));

            // Get locations
            var locations = await _context.Meetings
                .Where(m => m.IsActive && EF.Functions.Like(m.Location.Value, $"%{query}%"))
                .Select(m => m.Location.Value)
                .Distinct()
                .Take(maxSuggestions / 4)
                .ToListAsync(cancellationToken);

            suggestions.AddRange(locations.Select(location => new SearchSuggestionDto
            {
                Text = location,
                Type = "Location",
                Count = 1
            }));

            return Result.Success(suggestions.Take(maxSuggestions).ToList());
        }
        catch (Exception ex)
        {
            return Result.Failure<List<SearchSuggestionDto>>($"Failed to get search suggestions: {ex.Message}");
        }
    }

    public async Task<Result<List<string>>> GetPopularSearchTermsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get popular search terms from actual search queries in the last 30 days
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            var popularTerms = await _context.SearchQueries
                .Where(sq => sq.SearchedAt >= thirtyDaysAgo)
                .Where(sq => !string.IsNullOrWhiteSpace(sq.Query))
                .Where(sq => sq.Query.Length >= 2) // Filter out very short queries
                .SelectMany(sq => sq.Query.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries))
                .Where(term => term.Length >= 2)
                .Where(term => !IsCommonWord(term)) // Filter out common words
                .GroupBy(term => term)
                .OrderByDescending(g => g.Count())
                .Take(count)
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);

            // If we don't have enough data yet, supplement with default terms
            if (popularTerms.Count < count)
            {
                var defaultTerms = new List<string>
                {
                    "meeting", "discussion", "project", "team", "update", 
                    "review", "planning", "announcement", "feedback", "collaboration",
                    "schedule", "agenda", "presentation", "workshop", "conference"
                };

                var remainingCount = count - popularTerms.Count;
                var supplementaryTerms = defaultTerms
                    .Where(term => !popularTerms.Contains(term))
                    .Take(remainingCount);

                popularTerms.AddRange(supplementaryTerms);
            }

            return Result.Success(popularTerms);
        }
        catch (Exception ex)
        {
            return Result.Failure<List<string>>($"Failed to get popular search terms: {ex.Message}");
        }
    }

    #region Private Helper Methods

    private static List<string> ParseSearchQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return new List<string>();

        // Split by spaces and remove empty entries
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length >= 2)
            .Select(term => term.Trim())
            .Distinct()
            .ToList();

        return terms;
    }

    private static bool IsCommonWord(string word)
    {
        // Filter out common English words that aren't useful for search analytics
        var commonWords = new HashSet<string>
        {
            "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by",
            "from", "as", "is", "was", "are", "were", "be", "been", "have", "has", "had",
            "do", "does", "did", "will", "would", "should", "could", "can", "may", "might",
            "this", "that", "these", "those", "a", "an", "what", "when", "where", "who",
            "why", "how", "i", "you", "he", "she", "it", "we", "they", "me", "him", "her",
            "us", "them", "my", "your", "his", "our", "their", "about", "up", "down",
            "out", "off", "over", "under", "again", "further", "then", "once"
        };

        return commonWords.Contains(word.ToLower());
    }

    private static List<SearchResultDto> ApplySorting(List<SearchResultDto> results, string? sortBy, string? sortDirection)
    {
        var ascending = sortDirection?.ToLower() == "asc";

        return sortBy?.ToLower() switch
        {
            "date" => ascending 
                ? results.OrderBy(r => r.CreatedDate).ToList()
                : results.OrderByDescending(r => r.CreatedDate).ToList(),
            "title" => ascending
                ? results.OrderBy(r => r.Title).ToList()
                : results.OrderByDescending(r => r.Title).ToList(),
            _ => results.OrderByDescending(r => r.RelevanceScore).ThenByDescending(r => r.CreatedDate).ToList()
        };
    }

    private async Task<List<SearchResultDto>> SearchMeetingsInternalAsync(
        List<string> searchTerms,
        SearchFilters filters,
        CancellationToken cancellationToken)
    {
        var meetings = await _context.Meetings
            .Include(m => m.Creator)
            .Where(m => filters.ActiveOnly ? m.IsActive : true)
            .Where(m => searchTerms.Any(term =>
                EF.Functions.Like(m.Title, $"%{term}%") ||
                EF.Functions.Like(m.Description, $"%{term}%")))
            .ToListAsync(cancellationToken);

        return meetings.Select(m => new SearchResultDto
        {
            Id = m.Id.ToString(),
            Title = m.Title,
            Content = m.Description,
            Type = "Meeting",
            AuthorName = $"{m.Creator.FirstName} {m.Creator.LastName}",
            CreatedDate = m.CreatedDate,
            LastModifiedDate = m.LastModifiedDate,
            RelevanceScore = CalculateRelevanceScore(m.Title + " " + m.Description, searchTerms),
            Metadata = new Dictionary<string, object>
            {
                ["StartDateTime"] = m.MeetingDateTime.StartDateTime,
                ["Location"] = m.Location.Value
            }
        }).ToList();
    }

    private async Task<List<SearchResultDto>> SearchPostsInternalAsync(
        List<string> searchTerms,
        SearchFilters filters,
        CancellationToken cancellationToken)
    {
        var posts = await _context.Posts
            .Include(p => p.Author)
            .Include(p => p.Meeting)
            .Where(p => filters.ActiveOnly ? p.IsActive : true)
            .Where(p => searchTerms.Any(term =>
                EF.Functions.Like(p.Title, $"%{term}%") ||
                EF.Functions.Like(p.Content, $"%{term}%")))
            .ToListAsync(cancellationToken);

        return posts.Select(p => new SearchResultDto
        {
            Id = p.Id.ToString(),
            Title = p.Title,
            Content = p.Content,
            Type = "Post",
            AuthorName = $"{p.Author.FirstName} {p.Author.LastName}",
            CreatedDate = p.CreatedDate,
            LastModifiedDate = p.LastModifiedDate,
            RelevanceScore = CalculateRelevanceScore(p.Title + " " + p.Content, searchTerms),
            Metadata = new Dictionary<string, object>
            {
                ["MeetingTitle"] = p.Meeting.Title,
                ["MeetingId"] = p.MeetingId
            }
        }).ToList();
    }

    private async Task<List<SearchResultDto>> SearchCommentsInternalAsync(
        List<string> searchTerms,
        SearchFilters filters,
        CancellationToken cancellationToken)
    {
        var comments = await _context.Comments
            .Include(c => c.Author)
            .Include(c => c.Post)
            .Where(c => filters.ActiveOnly ? c.IsActive : true)
            .Where(c => searchTerms.Any(term =>
                EF.Functions.Like(c.Content, $"%{term}%")))
            .ToListAsync(cancellationToken);

        return comments.Select(c => new SearchResultDto
        {
            Id = c.Id.ToString(),
            Title = $"Comment on: {c.Post.Title}",
            Content = c.Content,
            Type = "Comment",
            AuthorName = $"{c.Author.FirstName} {c.Author.LastName}",
            CreatedDate = c.CreatedDate,
            LastModifiedDate = c.LastModifiedDate,
            RelevanceScore = CalculateRelevanceScore(c.Content, searchTerms),
            Metadata = new Dictionary<string, object>
            {
                ["PostTitle"] = c.Post.Title,
                ["PostId"] = c.PostId,
                ["IsReply"] = c.ParentCommentId.HasValue
            }
        }).ToList();
    }

    private async Task<List<SearchResultDto>> SearchUsersInternalAsync(
        List<string> searchTerms,
        SearchFilters filters,
        CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .Where(u => filters.ActiveOnly ? u.IsActive : true)
            .Where(u => searchTerms.Any(term =>
                EF.Functions.Like(u.FirstName, $"%{term}%") ||
                EF.Functions.Like(u.LastName, $"%{term}%") ||
                EF.Functions.Like(u.Email.Value, $"%{term}%")))
            .ToListAsync(cancellationToken);

        return users.Select(u => new SearchResultDto
        {
            Id = u.Id.ToString(),
            Title = $"{u.FirstName} {u.LastName}",
            Content = u.Email.Value,
            Type = "User",
            AuthorName = $"{u.FirstName} {u.LastName}",
            CreatedDate = u.CreatedDate,
            LastModifiedDate = u.LastModifiedDate,
            RelevanceScore = CalculateRelevanceScore($"{u.FirstName} {u.LastName} {u.Email.Value}", searchTerms),
            Metadata = new Dictionary<string, object>
            {
                ["Email"] = u.Email.Value,
                ["FullName"] = $"{u.FirstName} {u.LastName}"
            }
        }).ToList();
    }

    private static double CalculateRelevanceScore(string text, List<string> searchTerms)
    {
        if (string.IsNullOrWhiteSpace(text) || !searchTerms.Any())
            return 0;

        var textLower = text.ToLower();
        var score = 0.0;

        foreach (var term in searchTerms)
        {
            var termLower = term.ToLower();
            
            // Exact match gets highest score
            if (textLower == termLower)
                score += 100;
            
            // Title contains term gets high score
            else if (textLower.Contains(termLower))
            {
                // Word boundary match gets higher score than partial match
                var wordBoundaryPattern = $@"\b{Regex.Escape(termLower)}\b";
                if (Regex.IsMatch(textLower, wordBoundaryPattern))
                    score += 50;
                else
                    score += 25;
            }
        }

        return score;
    }

    private async Task TrackSearchQueryAsync(
        string query,
        string searchType,
        Guid? userId,
        int resultCount,
        TimeSpan searchDuration,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchQuery = SearchQuery.Create(
                query,
                searchType,
                userId,
                resultCount,
                searchDuration);

            _context.SearchQueries.Add(searchQuery);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Silently ignore tracking errors to not impact search functionality
        }
    }

    #endregion
}
