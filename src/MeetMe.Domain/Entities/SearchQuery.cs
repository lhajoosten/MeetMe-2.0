using MeetMe.Domain.Common;

namespace MeetMe.Domain.Entities;

/// <summary>
/// Represents a search query performed by a user for analytics and popular terms tracking
/// </summary>
public class SearchQuery : BaseEntity<int>
{
    public string Query { get; private set; } = string.Empty;
    public string SearchType { get; private set; } = string.Empty; // Global, Meeting, Post, Comment, User
    public Guid? UserId { get; private set; }
    public int ResultCount { get; private set; }
    public TimeSpan SearchDuration { get; private set; }
    public DateTime SearchedAt { get; private set; }
    public string IpAddress { get; private set; } = string.Empty;
    public string UserAgent { get; private set; } = string.Empty;
    
    // Navigation properties
    public User? User { get; private set; }

    private SearchQuery() { } // EF Core

    public static SearchQuery Create(
        string query,
        string searchType,
        Guid? userId = null,
        int resultCount = 0,
        TimeSpan searchDuration = default,
        string ipAddress = "",
        string userAgent = "")
    {
        var searchQuery = new SearchQuery
        {
            Query = query?.Trim() ?? string.Empty,
            SearchType = searchType,
            UserId = userId,
            ResultCount = resultCount,
            SearchDuration = searchDuration,
            SearchedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        };

        return searchQuery;
    }

    public void UpdateResults(int resultCount, TimeSpan searchDuration)
    {
        ResultCount = resultCount;
        SearchDuration = searchDuration;
    }
}
