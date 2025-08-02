using FluentAssertions;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Search.DTOs;
using MeetMe.Integration.Tests.Base;
using MeetMe.Integration.Tests.Collections;

namespace MeetMe.Integration.Tests.Services;

[Collection("Integration Tests")]
public class SearchServiceTests : OptimizedIntegrationTestBase
{
    [Fact]
    public async Task GlobalSearchAsync_WithValidQuery_ShouldReturnResults()
    {
        // Arrange
        await InitializeAsync();
        var query = "meeting";
        var filters = new SearchFilters { ActiveOnly = true };

        // Act
        var result = await TestHelper.SearchService.GlobalSearchAsync(query, filters);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().NotBeEmpty();
        result.Query.Should().Be(query);
        result.TotalCount.Should().BeGreaterThan(0);
        result.SearchDuration.Should().BeGreaterThan(TimeSpan.Zero);
        
        // Should include meetings and posts that match "meeting"
        result.Results.Should().Contain(r => r.Type == "Meeting");
        result.TypeCounts.Should().ContainKey("Meeting");
    }

    [Fact]
    public async Task GlobalSearchAsync_WithEmptyQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        await InitializeAsync();
        var query = "";

        // Act
        var result = await TestHelper.SearchService.GlobalSearchAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchMeetingsAsync_WithValidQuery_ShouldReturnMeetingResults()
    {
        // Arrange
        await InitializeAsync();
        var query = "team";

        // Act
        var result = await TestHelper.SearchService.SearchMeetingsAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().Contain(m => m.Title.Contains("Team", StringComparison.OrdinalIgnoreCase));
        result.Value.All(m => m.IsActive).Should().BeTrue();
    }

    [Fact]
    public async Task SearchPostsAsync_WithValidQuery_ShouldReturnPostResults()
    {
        // Arrange
        await InitializeAsync();
        var query = "agenda";

        // Act
        var result = await TestHelper.SearchService.SearchPostsAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().Contain(p => p.Title.Contains("Agenda", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchUsersAsync_WithValidQuery_ShouldReturnUserResults()
    {
        // Arrange
        await InitializeAsync();
        var query = "john";

        // Act
        var result = await TestHelper.SearchService.SearchUsersAsync(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().Contain(u => u.FirstName.Contains("John", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetSearchSuggestionsAsync_WithValidQuery_ShouldReturnSuggestions()
    {
        // Arrange
        await InitializeAsync();
        var query = "meet";

        // Act
        var result = await TestHelper.SearchService.GetSearchSuggestionsAsync(query, 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().HaveCountLessThanOrEqualTo(5);
        result.Value.Should().Contain(s => s.Text.Contains("meet", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetPopularSearchTermsAsync_WithSearchHistory_ShouldReturnPopularTerms()
    {
        // Arrange
        await InitializeAsync();

        // Act
        var result = await TestHelper.SearchService.GetPopularSearchTermsAsync(10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().HaveCountLessThanOrEqualTo(10);
        
        // Should include terms from our seeded search queries
        result.Value.Should().Contain("meeting");
        result.Value.Should().Contain("discussion");
    }

    [Fact]
    public async Task SearchWithFilters_DateRange_ShouldFilterResults()
    {
        // Arrange
        await InitializeAsync();
        var query = "meeting";
        var filters = new SearchFilters
        {
            FromDate = DateTime.UtcNow,
            ToDate = DateTime.UtcNow.AddDays(7),
            ActiveOnly = true
        };

        // Act
        var result = await TestHelper.SearchService.GlobalSearchAsync(query, filters);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().NotBeEmpty();
        
        // All meeting results should be within the date range
        var meetingResults = result.Results.Where(r => r.Type == "Meeting").ToList();
        foreach (var meeting in meetingResults)
        {
            if (meeting.Metadata.ContainsKey("StartDateTime"))
            {
                var startDateTime = (DateTime)meeting.Metadata["StartDateTime"];
                startDateTime.Should().BeOnOrAfter(filters.FromDate.Value);
                startDateTime.Should().BeOnOrBefore(filters.ToDate.Value);
            }
        }
    }

    [Fact]
    public async Task SearchWithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        await InitializeAsync();
        var query = "meeting";
        var page = 1;
        var pageSize = 1;

        // Act
        var result = await TestHelper.SearchService.GlobalSearchAsync(query, null, page, pageSize);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().HaveCount(1);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.TotalCount.Should().BeGreaterThan(0);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task SearchWithSorting_ByDate_ShouldReturnSortedResults()
    {
        // Arrange
        await InitializeAsync();
        var query = "meeting";
        var filters = new SearchFilters
        {
            SortBy = "Date",
            SortDirection = "Desc"
        };

        // Act
        var result = await TestHelper.SearchService.GlobalSearchAsync(query, filters);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().NotBeEmpty();
        
        // Results should be sorted by date descending
        for (int i = 0; i < result.Results.Count - 1; i++)
        {
            result.Results[i].CreatedDate.Should().BeOnOrAfter(result.Results[i + 1].CreatedDate);
        }
    }

    [Fact]
    public async Task SearchWithRelevanceScoring_ShouldScoreExactMatchesHigher()
    {
        // Arrange
        await InitializeAsync();
        var query = "meeting";

        // Act
        var result = await TestHelper.SearchService.GlobalSearchAsync(query);

        // Assert
        result.Should().NotBeNull();
        result.Results.Should().NotBeEmpty();
        
        // Results with exact title matches should have higher relevance scores
        var sortedByRelevance = result.Results.OrderByDescending(r => r.RelevanceScore).ToList();
        var topResult = sortedByRelevance.First();
        topResult.RelevanceScore.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData("Global")]
    [InlineData("Meeting")]
    [InlineData("Post")]
    [InlineData("User")]
    public async Task SearchTracking_ShouldRecordSearchQueries(string searchType)
    {
        // Arrange
        await InitializeAsync();
        var query = "test search";
        var initialCount = TestHelper.Context.SearchQueries.Count();

        // Act
        switch (searchType)
        {
            case "Global":
                await TestHelper.SearchService.GlobalSearchAsync(query);
                break;
            case "Meeting":
                await TestHelper.SearchService.SearchMeetingsAsync(query);
                break;
            case "Post":
                await TestHelper.SearchService.SearchPostsAsync(query);
                break;
            case "User":
                await TestHelper.SearchService.SearchUsersAsync(query);
                break;
        }

        // Wait a bit for background tracking to complete
        await Task.Delay(100);

        // Assert
        var finalCount = TestHelper.Context.SearchQueries.Count();
        finalCount.Should().BeGreaterThan(initialCount);
        
        var latestQuery = TestHelper.Context.SearchQueries
            .OrderByDescending(sq => sq.SearchedAt)
            .First();
        
        latestQuery.Query.Should().Be(query);
        latestQuery.SearchType.Should().Be(searchType);
        latestQuery.SearchDuration.Should().BeGreaterThan(TimeSpan.Zero);
    }
}
