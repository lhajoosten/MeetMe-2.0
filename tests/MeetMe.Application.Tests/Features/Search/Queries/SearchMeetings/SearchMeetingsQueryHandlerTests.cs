using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Search.DTOs;
using MeetMe.Application.Features.Search.Queries.SearchMeetings;
using Moq;

namespace MeetMe.Application.Tests.Features.Search.Queries.SearchMeetings;

public class SearchMeetingsQueryHandlerTests
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly SearchMeetingsQueryHandler _handler;

    public SearchMeetingsQueryHandlerTests()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _handler = new SearchMeetingsQueryHandler(_searchServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnMeetingResults()
    {
        // Arrange
        var query = new SearchMeetingsQuery("team meeting", new SearchFilters(), 1, 10);
        var expectedMeetings = new List<MeetingSearchResultDto>
        {
            new MeetingSearchResultDto
            {
                Id = 1,
                Title = "Weekly Team Meeting",
                Description = "Team standup and planning",
                StartDateTime = DateTime.UtcNow.AddDays(1),
                EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(1),
                Location = "Conference Room A",
                OrganizerName = "John Doe",
                AttendeeCount = 8,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-7)
            },
            new MeetingSearchResultDto
            {
                Id = 2,
                Title = "Project Team Meeting",
                Description = "Project status review",
                StartDateTime = DateTime.UtcNow.AddDays(2),
                EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(2),
                Location = "Conference Room B",
                OrganizerName = "Jane Smith",
                AttendeeCount = 12,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-5)
            }
        };
        var expectedResult = Result.Success(expectedMeetings);

        _searchServiceMock
            .Setup(x => x.SearchMeetingsAsync(
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
        result.Value.Should().BeEquivalentTo(expectedMeetings);

        _searchServiceMock.Verify(
            x => x.SearchMeetingsAsync(
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
        var query = new SearchMeetingsQuery("", new SearchFilters(), 1, 10);
        var expectedResult = Result.Success(new List<MeetingSearchResultDto>());

        _searchServiceMock
            .Setup(x => x.SearchMeetingsAsync(
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
    public async Task Handle_WithFilters_ShouldPassFiltersCorrectly()
    {
        // Arrange
        var filters = new SearchFilters
        {
            FromDate = DateTime.UtcNow.AddDays(-30),
            ToDate = DateTime.UtcNow.AddDays(30),
            Authors = new List<string> { "john.doe@example.com" },
            ActiveOnly = true,
            SortBy = "Date",
            SortDirection = "Asc"
        };
        var query = new SearchMeetingsQuery("sprint", filters, 1, 20);
        var expectedResult = Result.Success(new List<MeetingSearchResultDto>());

        _searchServiceMock
            .Setup(x => x.SearchMeetingsAsync(
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
            x => x.SearchMeetingsAsync(
                "sprint",
                It.Is<SearchFilters>(f =>
                    f.FromDate.HasValue &&
                    f.ToDate.HasValue &&
                    f.Authors.Contains("john.doe@example.com") &&
                    f.ActiveOnly == true &&
                    f.SortBy == "Date" &&
                    f.SortDirection == "Asc"),
                1,
                20,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithServiceFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new SearchMeetingsQuery("test", new SearchFilters(), 1, 10);
        var failureMessage = "Meeting search service error";
        var failureResult = Result.Failure<List<MeetingSearchResultDto>>(failureMessage);

        _searchServiceMock
            .Setup(x => x.SearchMeetingsAsync(
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
        var query = new SearchMeetingsQuery("test", new SearchFilters(), 1, 10);
        var exceptionMessage = "Database connection timeout";

        _searchServiceMock
            .Setup(x => x.SearchMeetingsAsync(
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
        result.Error.Should().Contain("Meeting search failed");
        result.Error.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task Handle_WithCancellation_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new SearchMeetingsQuery("test", new SearchFilters(), 1, 10);
        var cancellationToken = new CancellationToken(true);

        _searchServiceMock
            .Setup(x => x.SearchMeetingsAsync(
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
        result.Error.Should().Contain("Meeting search failed");

        _searchServiceMock.Verify(
            x => x.SearchMeetingsAsync(
                query.Query,
                query.Filters,
                query.Page,
                query.PageSize,
                cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(1, 20)]
    [InlineData(2, 15)]
    [InlineData(3, 5)]
    public async Task Handle_WithDifferentPaginationParameters_ShouldPassCorrectValues(int page, int pageSize)
    {
        // Arrange
        var query = new SearchMeetingsQuery("test", new SearchFilters(), page, pageSize);
        var expectedResult = Result.Success(new List<MeetingSearchResultDto>());

        _searchServiceMock
            .Setup(x => x.SearchMeetingsAsync(
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
            x => x.SearchMeetingsAsync(
                "test",
                It.IsAny<SearchFilters>(),
                page,
                pageSize,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithUpcomingMeetings_ShouldReturnCorrectlyOrderedResults()
    {
        // Arrange
        var query = new SearchMeetingsQuery("meeting", new SearchFilters(), 1, 10);
        var upcomingMeetings = new List<MeetingSearchResultDto>
        {
            new MeetingSearchResultDto
            {
                Id = 1,
                Title = "Tomorrow's Meeting",
                Description = "Early meeting",
                StartDateTime = DateTime.UtcNow.AddDays(1),
                EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(1),
                Location = "Online",
                OrganizerName = "Alice",
                AttendeeCount = 5,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-2)
            },
            new MeetingSearchResultDto
            {
                Id = 2,
                Title = "Next Week's Meeting",
                Description = "Future meeting",
                StartDateTime = DateTime.UtcNow.AddDays(7),
                EndDateTime = DateTime.UtcNow.AddDays(7).AddHours(2),
                Location = "Conference Room",
                OrganizerName = "Bob",
                AttendeeCount = 10,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-1)
            }
        };
        var expectedResult = Result.Success(upcomingMeetings);

        _searchServiceMock
            .Setup(x => x.SearchMeetingsAsync(
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
        result.Value[0].Id.Should().Be(1);
        result.Value[0].Title.Should().Be("Tomorrow's Meeting");
        result.Value[1].Id.Should().Be(2);
        result.Value[1].Title.Should().Be("Next Week's Meeting");
        
        // Verify all meetings are active and upcoming
        result.Value.Should().AllSatisfy(meeting =>
        {
            meeting.IsActive.Should().BeTrue();
            meeting.StartDateTime.Should().BeAfter(DateTime.UtcNow);
        });
    }
}
