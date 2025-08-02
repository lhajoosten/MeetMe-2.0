namespace MeetMe.Application.Features.Search.DTOs;

/// <summary>
/// Represents a unified search result that can contain different types of content
/// </summary>
public class SearchResultDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Meeting", "Post", "Comment", "User"
    public string AuthorName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public double RelevanceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Paginated search results with metadata
/// </summary>
public class SearchResultsDto
{
    public List<SearchResultDto> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
    public string Query { get; set; } = string.Empty;
    public TimeSpan SearchDuration { get; set; }
    public Dictionary<string, int> TypeCounts { get; set; } = new();
}

/// <summary>
/// Search filter options
/// </summary>
public class SearchFilters
{
    public List<string> Types { get; set; } = new(); // Meeting, Post, Comment, User
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<string> Authors { get; set; } = new();
    public bool ActiveOnly { get; set; } = true;
    public string SortBy { get; set; } = "Relevance"; // Relevance, Date, Title
    public string SortDirection { get; set; } = "Desc"; // Asc, Desc
}

/// <summary>
/// Search suggestion for autocomplete
/// </summary>
public class SearchSuggestionDto
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
}

/// <summary>
/// User-specific search result
/// </summary>
public class UserSearchResultDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public DateTime CreatedDate { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Meeting-specific search result with additional meeting metadata
/// </summary>
public class MeetingSearchResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public string Location { get; set; } = string.Empty;
    public string OrganizerName { get; set; } = string.Empty;
    public int AttendeeCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Post-specific search result with additional post metadata
/// </summary>
public class PostSearchResultDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int MeetingId { get; set; }
    public string MeetingTitle { get; set; } = string.Empty;
    public int CommentCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>
/// Comment-specific search result with additional comment metadata
/// </summary>
public class CommentSearchResultDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int PostId { get; set; }
    public string PostTitle { get; set; } = string.Empty;
    public int? ParentCommentId { get; set; }
    public bool IsReply { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}
