using MeetMe.Domain.Entities;

namespace MeetMe.Domain.Tests.Entities;

public class SearchQueryTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateSearchQuery()
    {
        // Arrange
        var query = "meeting discussion";
        var searchType = "Global";
        var userId = 1;
        var resultCount = 5;
        var searchDuration = TimeSpan.FromMilliseconds(150);
        var ipAddress = "192.168.1.1";
        var userAgent = "Mozilla/5.0";

        // Act
        var searchQuery = SearchQuery.Create(
            query,
            searchType,
            userId,
            resultCount,
            searchDuration,
            ipAddress,
            userAgent);

        // Assert
        Assert.NotNull(searchQuery);
        Assert.Equal(query, searchQuery.Query);
        Assert.Equal(searchType, searchQuery.SearchType);
        Assert.Equal(userId, searchQuery.UserId);
        Assert.Equal(resultCount, searchQuery.ResultCount);
        Assert.Equal(searchDuration, searchQuery.SearchDuration);
        Assert.Equal(ipAddress, searchQuery.IpAddress);
        Assert.Equal(userAgent, searchQuery.UserAgent);
        Assert.True(searchQuery.SearchedAt <= DateTime.UtcNow);
        Assert.True(searchQuery.SearchedAt > DateTime.UtcNow.AddMinutes(-1));
    }

    [Fact]
    public void Create_WithMinimalParameters_ShouldCreateSearchQuery()
    {
        // Arrange
        var query = "test";
        var searchType = "Meeting";

        // Act
        var searchQuery = SearchQuery.Create(query, searchType);

        // Assert
        Assert.NotNull(searchQuery);
        Assert.Equal(query, searchQuery.Query);
        Assert.Equal(searchType, searchQuery.SearchType);
        Assert.Null(searchQuery.UserId);
        Assert.Equal(0, searchQuery.ResultCount);
        Assert.Equal(TimeSpan.Zero, searchQuery.SearchDuration);
        Assert.Empty(searchQuery.IpAddress);
        Assert.Empty(searchQuery.UserAgent);
    }

    [Fact]
    public void Create_WithNullQuery_ShouldCreateEmptyQuery()
    {
        // Arrange
        string? query = null;
        var searchType = "Post";

        // Act
        var searchQuery = SearchQuery.Create(query!, searchType);

        // Assert
        Assert.NotNull(searchQuery);
        Assert.Empty(searchQuery.Query);
        Assert.Equal(searchType, searchQuery.SearchType);
    }

    [Fact]
    public void Create_WithWhitespaceQuery_ShouldTrimQuery()
    {
        // Arrange
        var query = "  project planning  ";
        var searchType = "Global";

        // Act
        var searchQuery = SearchQuery.Create(query, searchType);

        // Assert
        Assert.NotNull(searchQuery);
        Assert.Equal("project planning", searchQuery.Query);
    }

    [Theory]
    [InlineData("Global")]
    [InlineData("Meeting")]
    [InlineData("Post")]
    [InlineData("Comment")]
    [InlineData("User")]
    public void Create_WithDifferentSearchTypes_ShouldSetCorrectType(string searchType)
    {
        // Arrange
        var query = "test query";

        // Act
        var searchQuery = SearchQuery.Create(query, searchType);

        // Assert
        Assert.Equal(searchType, searchQuery.SearchType);
    }

    [Fact]
    public void UpdateResults_ShouldUpdateResultCountAndDuration()
    {
        // Arrange
        var searchQuery = SearchQuery.Create("test", "Global");
        var newResultCount = 10;
        var newDuration = TimeSpan.FromMilliseconds(250);

        // Act
        searchQuery.UpdateResults(newResultCount, newDuration);

        // Assert
        Assert.Equal(newResultCount, searchQuery.ResultCount);
        Assert.Equal(newDuration, searchQuery.SearchDuration);
    }

    [Fact]
    public void SearchedAt_ShouldBeSetToCurrentUtcTime()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var searchQuery = SearchQuery.Create("test", "Global");
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.True(searchQuery.SearchedAt >= beforeCreation);
        Assert.True(searchQuery.SearchedAt <= afterCreation);
    }

    [Fact]
    public void SearchQuery_ShouldHavePrivateParameterlessConstructor()
    {
        // This test ensures EF Core can instantiate the entity
        // The private constructor should be accessible to EF Core
        var constructors = typeof(SearchQuery).GetConstructors(
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);

        Assert.Contains(constructors, c => c.GetParameters().Length == 0);
    }
}
