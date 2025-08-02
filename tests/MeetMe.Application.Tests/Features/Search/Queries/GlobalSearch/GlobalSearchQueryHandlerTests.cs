using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Features.Search.DTOs;
using MeetMe.Application.Features.Search.Queries.GlobalSearch;
using Moq;

namespace MeetMe.Application.Tests.Features.Search.Queries.GlobalSearch;

public class GlobalSearchQueryHandlerTests
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly GlobalSearchQueryHandler _handler;

    public GlobalSearchQueryHandlerTests()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _handler = new GlobalSearchQueryHandler(_searchServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnSearchResults()
    {
        // Arrange
        var query = new GlobalSearchQuery("test meeting", new SearchFilters(), 1, 10);
        var expectedResults = new SearchResultsDto
        {
            Results = new List<SearchResultDto>
            {
                new SearchResultDto
                {
                    Id = "1",
                    Title = "Test Meeting",
                    Content = "This is a test meeting",
                    Type = "Meeting",
                    AuthorName = "John Doe",
                    CreatedDate = DateTime.UtcNow,
                    RelevanceScore = 0.95
                }
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 10,
            Query = "test meeting",
            SearchDuration = TimeSpan.FromMilliseconds(100),
            TypeCounts = new Dictionary<string, int> { { "Meeting", 1 } }
        };

        _searchServiceMock
            .Setup(x => x.GlobalSearchAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedResults);
        
        _searchServiceMock.Verify(
            x => x.GlobalSearchAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        var query = new GlobalSearchQuery("", new SearchFilters(), 1, 10);
        var expectedResults = new SearchResultsDto
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 10,
            Query = "",
            SearchDuration = TimeSpan.FromMilliseconds(50),
            TypeCounts = new Dictionary<string, int>()
        };

        _searchServiceMock
            .Setup(x => x.GlobalSearchAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Results.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
        result.Value.Query.Should().Be("");
    }

    [Fact]
    public async Task Handle_WithFilters_ShouldPassFiltersCorrectly()
    {
        // Arrange
        var filters = new SearchFilters
        {
            Types = new List<string> { "Meeting", "Post" },
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow,
            Authors = new List<string> { "john.doe@example.com" },
            ActiveOnly = true,
            SortBy = "Date",
            SortDirection = "Desc"
        };
        var query = new GlobalSearchQuery("important", filters, 1, 20);
        var expectedResults = new SearchResultsDto
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            Page = 1,
            PageSize = 20,
            Query = "important",
            SearchDuration = TimeSpan.FromMilliseconds(75),
            TypeCounts = new Dictionary<string, int>()
        };

        _searchServiceMock
            .Setup(x => x.GlobalSearchAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        
        _searchServiceMock.Verify(
            x => x.GlobalSearchAsync(
                "important",
                It.Is<SearchFilters>(f => 
                    f.Types.Count == 2 &&
                    f.Types.Contains("Meeting") &&
                    f.Types.Contains("Post") &&
                    f.ActiveOnly == true &&
                    f.SortBy == "Date"),
                1,
                20,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleResultTypes_ShouldReturnMixedResults()
    {
        // Arrange
        var query = new GlobalSearchQuery("collaboration", new SearchFilters(), 1, 15);
        var expectedResults = new SearchResultsDto
        {
            Results = new List<SearchResultDto>
            {
                new SearchResultDto
                {
                    Id = "1",
                    Title = "Team Collaboration Meeting",
                    Content = "Discussing collaboration strategies",
                    Type = "Meeting",
                    AuthorName = "Alice Smith",
                    CreatedDate = DateTime.UtcNow.AddDays(-1),
                    RelevanceScore = 0.92
                },
                new SearchResultDto
                {
                    Id = "2",
                    Title = "Collaboration Best Practices",
                    Content = "Post about effective collaboration methods",
                    Type = "Post",
                    AuthorName = "Bob Johnson",
                    CreatedDate = DateTime.UtcNow.AddDays(-2),
                    RelevanceScore = 0.88
                },
                new SearchResultDto
                {
                    Id = "3",
                    Title = "Great collaboration ideas!",
                    Content = "Comment on collaboration techniques",
                    Type = "Comment",
                    AuthorName = "Carol Davis",
                    CreatedDate = DateTime.UtcNow.AddDays(-3),
                    RelevanceScore = 0.75
                }
            },
            TotalCount = 3,
            Page = 1,
            PageSize = 15,
            Query = "collaboration",
            SearchDuration = TimeSpan.FromMilliseconds(120),
            TypeCounts = new Dictionary<string, int> 
            { 
                { "Meeting", 1 }, 
                { "Post", 1 }, 
                { "Comment", 1 } 
            }
        };

        _searchServiceMock
            .Setup(x => x.GlobalSearchAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Results.Should().HaveCount(3);
        result.Value.TypeCounts.Should().HaveCount(3);
        result.Value.TypeCounts["Meeting"].Should().Be(1);
        result.Value.TypeCounts["Post"].Should().Be(1);
        result.Value.TypeCounts["Comment"].Should().Be(1);
        result.Value.SearchDuration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPageInfo()
    {
        // Arrange
        var query = new GlobalSearchQuery("test", new SearchFilters(), 2, 5);
        var expectedResults = new SearchResultsDto
        {
            Results = new List<SearchResultDto>
            {
                new SearchResultDto
                {
                    Id = "6",
                    Title = "Test Item 6",
                    Content = "Content for test item 6",
                    Type = "Meeting",
                    AuthorName = "User Six",
                    CreatedDate = DateTime.UtcNow.AddDays(-6),
                    RelevanceScore = 0.65
                }
            },
            TotalCount = 11,
            Page = 2,
            PageSize = 5,
            Query = "test",
            SearchDuration = TimeSpan.FromMilliseconds(95),
            TypeCounts = new Dictionary<string, int> { { "Meeting", 11 } }
        };

        _searchServiceMock
            .Setup(x => x.GlobalSearchAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(2);
        result.Value.PageSize.Should().Be(5);
        result.Value.TotalCount.Should().Be(11);
        result.Value.TotalPages.Should().Be(3); // Math.Ceiling(11/5) = 3
        result.Value.HasPreviousPage.Should().BeTrue();
        result.Value.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithCancellation_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new GlobalSearchQuery("test", new SearchFilters(), 1, 10);
        var cancellationToken = new CancellationToken(true);

        _searchServiceMock
            .Setup(x => x.GlobalSearchAsync(
                It.IsAny<string>(),
                It.IsAny<SearchFilters>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                cancellationToken))
            .ThrowsAsync(new OperationCanceledException());

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Search failed");

        _searchServiceMock.Verify(
            x => x.GlobalSearchAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithServiceException_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new GlobalSearchQuery("test", new SearchFilters(), 1, 10);
        var exceptionMessage = "Database connection failed";

        _searchServiceMock
            .Setup(x => x.GlobalSearchAsync(
                It.IsAny<string>(),
                It.IsAny<SearchFilters>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Search failed");
        result.Error.Should().Contain(exceptionMessage);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(1, 20)]
    [InlineData(2, 15)]
    [InlineData(5, 5)]
    public async Task Handle_WithDifferentPaginationParameters_ShouldPassCorrectValues(int page, int pageSize)
    {
        // Arrange
        var query = new GlobalSearchQuery("test", new SearchFilters(), page, pageSize);
        var expectedResults = new SearchResultsDto
        {
            Results = new List<SearchResultDto>(),
            TotalCount = 0,
            Page = page,
            PageSize = pageSize,
            Query = "test",
            SearchDuration = TimeSpan.FromMilliseconds(50),
            TypeCounts = new Dictionary<string, int>()
        };

        _searchServiceMock
            .Setup(x => x.GlobalSearchAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResults);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Page.Should().Be(page);
        result.Value.PageSize.Should().Be(pageSize);
        
        _searchServiceMock.Verify(
            x => x.GlobalSearchAsync(
                "test",
                It.IsAny<SearchFilters>(),
                page,
                pageSize,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
