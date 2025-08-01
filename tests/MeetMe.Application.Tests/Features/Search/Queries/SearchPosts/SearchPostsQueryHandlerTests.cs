using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Search.Queries.SearchPosts;
using Moq;

namespace MeetMe.Application.Tests.Features.Search.Queries.SearchPosts;

public class SearchPostsQueryHandlerTests
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly SearchPostsQueryHandler _handler;

    public SearchPostsQueryHandlerTests()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _handler = new SearchPostsQueryHandler(_searchServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnPostResults()
    {
        // Arrange
        var query = new SearchPostsQuery("project update", new SearchFilters(), 1, 10);
        var expectedPosts = new List<PostSearchResultDto>
        {
            new PostSearchResultDto
            {
                Id = 1,
                Title = "Weekly Project Update",
                Content = "This week we made significant progress on the project...",
                AuthorName = "John Doe",
                MeetingId = 5,
                MeetingTitle = "Weekly Team Meeting",
                CommentCount = 3,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            },
            new PostSearchResultDto
            {
                Id = 2,
                Title = "Project Milestone Reached",
                Content = "We have successfully completed the first milestone...",
                AuthorName = "Jane Smith",
                MeetingId = 7,
                MeetingTitle = "Project Review Meeting",
                CommentCount = 8,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            }
        };
        var expectedResult = Result.Success(expectedPosts);

        _searchServiceMock
            .Setup(x => x.SearchPostsAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(expectedPosts);

        _searchServiceMock.Verify(
            x => x.SearchPostsAsync(
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
        var query = new SearchPostsQuery("", new SearchFilters(), 1, 10);
        var expectedResult = Result.Success(new List<PostSearchResultDto>());

        _searchServiceMock
            .Setup(x => x.SearchPostsAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithSpecificAuthorFilter_ShouldPassFiltersCorrectly()
    {
        // Arrange
        var filters = new SearchFilters
        {
            Authors = new List<string> { "john.doe@example.com" },
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow,
            ActiveOnly = true,
            SortBy = "Date",
            SortDirection = "Desc"
        };
        var query = new SearchPostsQuery("update", filters, 1, 15);
        var expectedResult = Result.Success(new List<PostSearchResultDto>());

        _searchServiceMock
            .Setup(x => x.SearchPostsAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _searchServiceMock.Verify(
            x => x.SearchPostsAsync(
                "update",
                It.Is<SearchFilters>(f =>
                    f.Authors.Contains("john.doe@example.com") &&
                    f.FromDate.HasValue &&
                    f.ToDate.HasValue &&
                    f.ActiveOnly == true &&
                    f.SortBy == "Date" &&
                    f.SortDirection == "Desc"),
                1,
                15,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithServiceFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new SearchPostsQuery("test", new SearchFilters(), 1, 10);
        var failureMessage = "Post search service error";
        var failureResult = Result.Failure<List<PostSearchResultDto>>(failureMessage);

        _searchServiceMock
            .Setup(x => x.SearchPostsAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(failureResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(failureMessage);
    }

    [Fact]
    public async Task Handle_WithServiceException_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new SearchPostsQuery("test", new SearchFilters(), 1, 10);
        var exceptionMessage = "Database query timeout";

        _searchServiceMock
            .Setup(x => x.SearchPostsAsync(
                It.IsAny<string>(),
                It.IsAny<SearchFilters>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TimeoutException(exceptionMessage));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Post search failed");
        result.Error.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task Handle_WithPostsFromDifferentMeetings_ShouldReturnCorrectResults()
    {
        // Arrange
        var query = new SearchPostsQuery("discussion", new SearchFilters(), 1, 10);
        var expectedPosts = new List<PostSearchResultDto>
        {
            new PostSearchResultDto
            {
                Id = 1,
                Title = "Discussion Points",
                Content = "Let's discuss the main points from today's meeting",
                AuthorName = "Alice Cooper",
                MeetingId = 10,
                MeetingTitle = "Team Standup",
                CommentCount = 5,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddHours(-2)
            },
            new PostSearchResultDto
            {
                Id = 2,
                Title = "Follow-up Discussion",
                Content = "Continuing our discussion from yesterday",
                AuthorName = "Bob Wilson",
                MeetingId = 12,
                MeetingTitle = "Architecture Review",
                CommentCount = 12,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddHours(-1)
            }
        };
        var expectedResult = Result.Success(expectedPosts);

        _searchServiceMock
            .Setup(x => x.SearchPostsAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        
        // Verify posts are from different meetings
        result.Value[0].MeetingId.Should().Be(10);
        result.Value[0].MeetingTitle.Should().Be("Team Standup");
        result.Value[1].MeetingId.Should().Be(12);
        result.Value[1].MeetingTitle.Should().Be("Architecture Review");
        
        // Verify all posts are active and have content
        result.Value.Should().AllSatisfy(post =>
        {
            post.IsActive.Should().BeTrue();
            post.Content.Should().NotBeNullOrEmpty();
            post.Title.Should().NotBeNullOrEmpty();
            post.AuthorName.Should().NotBeNullOrEmpty();
        });
    }

    [Theory]
    [InlineData("project")]
    [InlineData("update")]
    [InlineData("meeting notes")]
    [InlineData("action items")]
    public async Task Handle_WithDifferentContentSearchTerms_ShouldSearchCorrectly(string searchTerm)
    {
        // Arrange
        var query = new SearchPostsQuery(searchTerm, new SearchFilters(), 1, 10);
        var expectedResult = Result.Success(new List<PostSearchResultDto>());

        _searchServiceMock
            .Setup(x => x.SearchPostsAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _searchServiceMock.Verify(
            x => x.SearchPostsAsync(
                searchTerm,
                It.IsAny<SearchFilters>(),
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPopularPosts_ShouldReturnPostsWithComments()
    {
        // Arrange
        var query = new SearchPostsQuery("popular", new SearchFilters(), 1, 10);
        var expectedPosts = new List<PostSearchResultDto>
        {
            new PostSearchResultDto
            {
                Id = 1,
                Title = "Popular Discussion Topic",
                Content = "This post has generated a lot of discussion",
                AuthorName = "Community Manager",
                MeetingId = 20,
                MeetingTitle = "Community Meeting",
                CommentCount = 25,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-3)
            },
            new PostSearchResultDto
            {
                Id = 2,
                Title = "Another Popular Post",
                Content = "This is also very popular among team members",
                AuthorName = "Team Lead",
                MeetingId = 21,
                MeetingTitle = "Team Building",
                CommentCount = 18,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            }
        };
        var expectedResult = Result.Success(expectedPosts);

        _searchServiceMock
            .Setup(x => x.SearchPostsAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        
        // Verify posts have comment activity
        result.Value.Should().AllSatisfy(post =>
        {
            post.CommentCount.Should().BeGreaterThan(10);
            post.IsActive.Should().BeTrue();
        });
    }
}
