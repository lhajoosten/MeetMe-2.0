using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Search.Queries.GetSearchSuggestions;
using Moq;

namespace MeetMe.Application.Tests.Features.Search.Queries.GetSearchSuggestions;

public class GetSearchSuggestionsQueryHandlerTests
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly GetSearchSuggestionsQueryHandler _handler;

    public GetSearchSuggestionsQueryHandlerTests()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _handler = new GetSearchSuggestionsQueryHandler(_searchServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnSuggestions()
    {
        // Arrange
        var query = new GetSearchSuggestionsQuery("mee", 5);
        var expectedSuggestions = new List<SearchSuggestionDto>
        {
            new SearchSuggestionDto
            {
                Text = "meeting",
                Type = "keyword",
                Count = 45
            },
            new SearchSuggestionDto
            {
                Text = "meetings",
                Type = "keyword", 
                Count = 32
            },
            new SearchSuggestionDto
            {
                Text = "meet and greet",
                Type = "phrase",
                Count = 18
            },
            new SearchSuggestionDto
            {
                Text = "meet the team",
                Type = "phrase",
                Count = 12
            }
        };
        var expectedResult = Result.Success(expectedSuggestions);

        _searchServiceMock
            .Setup(x => x.GetSearchSuggestionsAsync(
                query.Query,
                query.MaxSuggestions,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(4);
        result.Value.Should().BeEquivalentTo(expectedSuggestions);

        _searchServiceMock.Verify(
            x => x.GetSearchSuggestionsAsync(
                query.Query,
                query.MaxSuggestions,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyQuery_ShouldReturnEmptyResults()
    {
        // Arrange
        var query = new GetSearchSuggestionsQuery("", 10);
        var expectedResult = Result.Success(new List<SearchSuggestionDto>());

        _searchServiceMock
            .Setup(x => x.GetSearchSuggestionsAsync(
                query.Query,
                query.MaxSuggestions,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    public async Task Handle_WithDifferentMaxSuggestions_ShouldPassCorrectLimit(int maxSuggestions)
    {
        // Arrange
        var query = new GetSearchSuggestionsQuery("test", maxSuggestions);
        var suggestions = new List<SearchSuggestionDto>();
        
        // Create the expected number of suggestions (up to the limit)
        for (int i = 1; i <= Math.Min(maxSuggestions, 10); i++)
        {
            suggestions.Add(new SearchSuggestionDto
            {
                Text = $"test suggestion {i}",
                Type = "keyword",
                Count = 10 - i
            });
        }
        
        var expectedResult = Result.Success(suggestions);

        _searchServiceMock
            .Setup(x => x.GetSearchSuggestionsAsync(
                query.Query,
                query.MaxSuggestions,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(Math.Min(maxSuggestions, 10));

        _searchServiceMock.Verify(
            x => x.GetSearchSuggestionsAsync(
                "test",
                maxSuggestions,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithPartialMatch_ShouldReturnRelevantSuggestions()
    {
        // Arrange
        var query = new GetSearchSuggestionsQuery("proj", 8);
        var expectedSuggestions = new List<SearchSuggestionDto>
        {
            new SearchSuggestionDto
            {
                Text = "project",
                Type = "keyword",
                Count = 67
            },
            new SearchSuggestionDto
            {
                Text = "project meeting",
                Type = "phrase",
                Count = 23
            },
            new SearchSuggestionDto
            {
                Text = "project update",
                Type = "phrase",
                Count = 19
            },
            new SearchSuggestionDto
            {
                Text = "project planning",
                Type = "phrase",
                Count = 15
            }
        };
        var expectedResult = Result.Success(expectedSuggestions);

        _searchServiceMock
            .Setup(x => x.GetSearchSuggestionsAsync(
                query.Query,
                query.MaxSuggestions,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(4);
        
        // Verify suggestions are ordered by count (most popular first)
        result.Value[0].Count.Should().BeGreaterThan(result.Value[1].Count);
        result.Value[1].Count.Should().BeGreaterThan(result.Value[2].Count);
        result.Value[2].Count.Should().BeGreaterThan(result.Value[3].Count);
        
        // Verify all suggestions contain the query term
        result.Value.Should().AllSatisfy(suggestion =>
        {
            suggestion.Text.Should().Contain("proj");
        });
    }

    [Fact]
    public async Task Handle_WithServiceFailure_ShouldReturnFailureResult()
    {
        // Arrange
        var query = new GetSearchSuggestionsQuery("test", 10);
        var failureMessage = "Suggestions service unavailable";
        var failureResult = Result.Failure<List<SearchSuggestionDto>>(failureMessage);

        _searchServiceMock
            .Setup(x => x.GetSearchSuggestionsAsync(
                query.Query,
                query.MaxSuggestions,
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
        var query = new GetSearchSuggestionsQuery("test", 10);
        var exceptionMessage = "Index not available";

        _searchServiceMock
            .Setup(x => x.GetSearchSuggestionsAsync(
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get search suggestions");
        result.Error.Should().Contain(exceptionMessage);
    }

    [Fact]
    public async Task Handle_WithDifferentSuggestionTypes_ShouldReturnMixedTypes()
    {
        // Arrange
        var query = new GetSearchSuggestionsQuery("team", 10);
        var expectedSuggestions = new List<SearchSuggestionDto>
        {
            new SearchSuggestionDto
            {
                Text = "team",
                Type = "keyword",
                Count = 89
            },
            new SearchSuggestionDto
            {
                Text = "team meeting",
                Type = "phrase",
                Count = 45
            },
            new SearchSuggestionDto
            {
                Text = "Team Alpha",
                Type = "entity",
                Count = 28
            },
            new SearchSuggestionDto
            {
                Text = "team building",
                Type = "phrase",
                Count = 22
            },
            new SearchSuggestionDto
            {
                Text = "Team Beta",
                Type = "entity",
                Count = 15
            }
        };
        var expectedResult = Result.Success(expectedSuggestions);

        _searchServiceMock
            .Setup(x => x.GetSearchSuggestionsAsync(
                query.Query,
                query.MaxSuggestions,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(5);
        
        // Verify different suggestion types are present
        result.Value.Should().Contain(s => s.Type == "keyword");
        result.Value.Should().Contain(s => s.Type == "phrase");
        result.Value.Should().Contain(s => s.Type == "entity");
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    public async Task Handle_WithShortQueries_ShouldStillReturnSuggestions(string shortQuery)
    {
        // Arrange
        var query = new GetSearchSuggestionsQuery(shortQuery, 5);
        var expectedSuggestions = new List<SearchSuggestionDto>
        {
            new SearchSuggestionDto
            {
                Text = $"{shortQuery}genda",
                Type = "keyword",
                Count = 15
            },
            new SearchSuggestionDto
            {
                Text = $"{shortQuery}ction items",
                Type = "phrase",
                Count = 12
            }
        };
        var expectedResult = Result.Success(expectedSuggestions);

        _searchServiceMock
            .Setup(x => x.GetSearchSuggestionsAsync(
                query.Query,
                query.MaxSuggestions,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _searchServiceMock.Verify(
            x => x.GetSearchSuggestionsAsync(
                shortQuery,
                5,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
