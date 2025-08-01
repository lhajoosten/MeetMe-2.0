using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Search.Queries.GetPopularSearchTerms;
using Moq;

namespace MeetMe.Application.Tests.Features.Search.Queries.GetPopularSearchTerms;

public class GetPopularSearchTermsQueryHandlerTests
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly GetPopularSearchTermsQueryHandler _handler;

    public GetPopularSearchTermsQueryHandlerTests()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _handler = new GetPopularSearchTermsQueryHandler(_searchServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithDefaultCount_ShouldReturnPopularTerms()
    {
        // Arrange
        var query = new GetPopularSearchTermsQuery(10);
        var expectedTerms = new List<string>
        {
            "meeting",
            "project",
            "team",
            "update",
            "discussion",
            "planning",
            "review",
            "standup",
            "sprint",
            "agenda"
        };
        var expectedResult = Result.Success(expectedTerms);

        _searchServiceMock
            .Setup(x => x.GetPopularSearchTermsAsync(
                query.Count,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(10);
        result.Value.Should().BeEquivalentTo(expectedTerms);

        _searchServiceMock.Verify(
            x => x.GetPopularSearchTermsAsync(
                query.Count,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public async Task Handle_WithDifferentCounts_ShouldReturnCorrectNumberOfTerms(int count)
    {
        // Arrange
        var query = new GetPopularSearchTermsQuery(count);
        var expectedTerms = new List<string>();
        
        // Generate expected terms up to the requested count
        var allTerms = new[] { "meeting", "project", "team", "update", "discussion", 
                              "planning", "review", "standup", "sprint", "agenda",
                              "action", "notes", "follow-up", "deadline", "milestone",
                              "collaborate", "sync", "check-in", "workshop", "presentation" };
        
        for (int i = 0; i < Math.Min(count, allTerms.Length); i++)
        {
            expectedTerms.Add(allTerms[i]);
        }
        
        var expectedResult = Result.Success(expectedTerms);

        _searchServiceMock
            .Setup(x => x.GetPopularSearchTermsAsync(
                query.Count,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(Math.Min(count, allTerms.Length));

        _searchServiceMock.Verify(
            x => x.GetPopularSearchTermsAsync(
                count,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNoPopularTerms_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetPopularSearchTermsQuery(10);
        var expectedResult = Result.Success(new List<string>());

        _searchServiceMock
            .Setup(x => x.GetPopularSearchTermsAsync(
                query.Count,
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
    public async Task Handle_WithServiceFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new GetPopularSearchTermsQuery(10);
        var failureMessage = "Failed to retrieve popular search terms";
        var failureResult = Result.Failure<List<string>>(failureMessage);

        _searchServiceMock
            .Setup(x => x.GetPopularSearchTermsAsync(
                query.Count,
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
    public async Task Handle_WithMostPopularTerms_ShouldReturnInCorrectOrder()
    {
        // Arrange
        var query = new GetPopularSearchTermsQuery(5);
        var expectedTerms = new List<string>
        {
            "meeting",      // Most popular
            "project", 
            "team",
            "update",
            "discussion"    // Least popular of the top 5
        };
        var expectedResult = Result.Success(expectedTerms);

        _searchServiceMock
            .Setup(x => x.GetPopularSearchTermsAsync(
                query.Count,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
        
        // Verify the most popular term is first
        result.Value[0].Should().Be("meeting");
        result.Value[1].Should().Be("project");
        result.Value[2].Should().Be("team");
        result.Value[3].Should().Be("update");
        result.Value[4].Should().Be("discussion");
    }

    [Fact]
    public async Task Handle_WithZeroCount_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetPopularSearchTermsQuery(0);
        var expectedResult = Result.Success(new List<string>());

        _searchServiceMock
            .Setup(x => x.GetPopularSearchTermsAsync(
                query.Count,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _searchServiceMock.Verify(
            x => x.GetPopularSearchTermsAsync(
                0,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithWorkplaceBuzzwords_ShouldReturnRelevantTerms()
    {
        // Arrange
        var query = new GetPopularSearchTermsQuery(8);
        var expectedTerms = new List<string>
        {
            "standup",
            "sprint",
            "retrospective",
            "scrum",
            "agile",
            "sync",
            "one-on-one",
            "all-hands"
        };
        var expectedResult = Result.Success(expectedTerms);

        _searchServiceMock
            .Setup(x => x.GetPopularSearchTermsAsync(
                query.Count,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(8);
        
        // Verify terms are workplace-related
        result.Value.Should().Contain(term => 
            term.Contains("standup") || 
            term.Contains("sprint") || 
            term.Contains("scrum") ||
            term.Contains("agile"));
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToService()
    {
        // Arrange
        var query = new GetPopularSearchTermsQuery(10);
        var cancellationToken = new CancellationToken();
        var expectedResult = Result.Success(new List<string> { "meeting", "project" });

        _searchServiceMock
            .Setup(x => x.GetPopularSearchTermsAsync(
                query.Count,
                cancellationToken))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _searchServiceMock.Verify(
            x => x.GetPopularSearchTermsAsync(
                10,
                cancellationToken),
            Times.Once);
    }
}
