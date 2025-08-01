using FluentAssertions;
using MeetMe.Application.Features.Search.Queries.GlobalSearch;
using MeetMe.Application.Features.Search.Queries.GetPopularSearchTerms;
using MeetMe.Application.Common.Models;
using MeetMe.Integration.Tests.Base;
using MeetMe.Integration.Tests.Collections;

namespace MeetMe.Integration.Tests.Features;

[Collection("Integration Tests")]
public class SearchQueriesTests : OptimizedIntegrationTestBase
{

    [Fact]
    public async Task GlobalSearchQuery_WithValidQuery_ShouldReturnResults()
    {
        // Arrange
        await InitializeAsync();
        var query = new GlobalSearchQuery("meeting", new SearchFilters(), 1, 10);

        // Act
        var result = await TestHelper.GetMediator().Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Results.Should().NotBeEmpty();
        result.Value.Query.Should().Be("meeting");
        result.Value.TotalCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetPopularSearchTermsQuery_ShouldReturnPopularTerms()
    {
        // Arrange
        await InitializeAsync();
        var query = new GetPopularSearchTermsQuery(5);

        // Act
        var result = await TestHelper.GetMediator().Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().NotBeEmpty();
        result.Value.Should().HaveCountLessThanOrEqualTo(5);
    }

    [Fact]
    public async Task GlobalSearchQuery_WithFilters_ShouldApplyFilters()
    {
        // Arrange
        await InitializeAsync();
        var filters = new SearchFilters
        {
            Types = new List<string> { "Meeting" },
            ActiveOnly = true,
            SortBy = "Date",
            SortDirection = "Desc"
        };
        var query = new GlobalSearchQuery("meeting", filters, 1, 10);

        // Act
        var result = await TestHelper.GetMediator().Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Results.Should().NotBeEmpty();
        result.Value.TypeCounts.Should().ContainKey("Meeting");
        
        // Should only contain meeting results
        result.Value.Results.Should().OnlyContain(r => r.Type == "Meeting");
    }

    [Fact]
    public async Task GlobalSearchQuery_WithPagination_ShouldRespectPagination()
    {
        // Arrange
        await InitializeAsync();
        var query = new GlobalSearchQuery("meeting", new SearchFilters(), 1, 1);

        // Act
        var result = await TestHelper.GetMediator().Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Results.Should().HaveCount(1);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(1);
        result.Value.HasNextPage.Should().BeTrue();
        result.Value.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GlobalSearchQuery_WithEmptyQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        await InitializeAsync();
        var query = new GlobalSearchQuery("", new SearchFilters(), 1, 10);

        // Act
        var result = await TestHelper.GetMediator().Send(query);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Results.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task SearchQuery_Performance_ShouldCompleteInReasonableTime()
    {
        // Arrange
        await InitializeAsync();
        var query = new GlobalSearchQuery("meeting", new SearchFilters(), 1, 20);

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await TestHelper.GetMediator().Send(query);
        stopwatch.Stop();

        // Assert
        result.IsSuccess.Should().BeTrue();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000); // Should complete within 1 second
        result.Value.SearchDuration.Should().BeGreaterThan(TimeSpan.Zero);
    }
}
