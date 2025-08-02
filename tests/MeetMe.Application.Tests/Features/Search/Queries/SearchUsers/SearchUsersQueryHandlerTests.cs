using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Search.DTOs;
using MeetMe.Application.Features.Search.Queries.SearchUsers;
using Moq;

namespace MeetMe.Application.Tests.Features.Search.Queries.SearchUsers;

public class SearchUsersQueryHandlerTests
{
    private readonly Mock<ISearchService> _searchServiceMock;
    private readonly SearchUsersQueryHandler _handler;

    public SearchUsersQueryHandlerTests()
    {
        _searchServiceMock = new Mock<ISearchService>();
        _handler = new SearchUsersQueryHandler(_searchServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnUserResults()
    {
        // Arrange
        var query = new SearchUsersQuery("john", new SearchFilters(), 1, 10);
        var expectedUsers = new List<UserSearchResultDto>
        {
            new UserSearchResultDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                CreatedDate = DateTime.UtcNow.AddDays(-30)
            },
            new UserSearchResultDto
            {
                Id = 1,
                FirstName = "Johnny",
                LastName = "Smith",
                Email = "johnny.smith@example.com",
                CreatedDate = DateTime.UtcNow.AddDays(-15)
            }
        };
        var expectedResult = Result.Success(expectedUsers);

        _searchServiceMock
            .Setup(x => x.SearchUsersAsync(
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
        result.Value.Should().BeEquivalentTo(expectedUsers);

        _searchServiceMock.Verify(
            x => x.SearchUsersAsync(
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
        var query = new SearchUsersQuery("", new SearchFilters(), 1, 10);
        var expectedResult = Result.Success(new List<UserSearchResultDto>());

        _searchServiceMock
            .Setup(x => x.SearchUsersAsync(
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
    public async Task Handle_WithActiveOnlyFilter_ShouldPassFiltersCorrectly()
    {
        // Arrange
        var filters = new SearchFilters
        {
            ActiveOnly = true,
            SortBy = "Name",
            SortDirection = "Asc"
        };
        var query = new SearchUsersQuery("user", filters, 1, 20);
        var expectedResult = Result.Success(new List<UserSearchResultDto>());

        _searchServiceMock
            .Setup(x => x.SearchUsersAsync(
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
            x => x.SearchUsersAsync(
                "user",
                It.Is<SearchFilters>(f =>
                    f.ActiveOnly == true &&
                    f.SortBy == "Name" &&
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
        var query = new SearchUsersQuery("test", new SearchFilters(), 1, 10);
        var failureMessage = "User search service error";
        var failureResult = Result.Failure<List<UserSearchResultDto>>(failureMessage);

        _searchServiceMock
            .Setup(x => x.SearchUsersAsync(
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
        var query = new SearchUsersQuery("test", new SearchFilters(), 1, 10);
        var exceptionMessage = "Authentication failed";

        _searchServiceMock
            .Setup(x => x.SearchUsersAsync(
                It.IsAny<string>(),
                It.IsAny<SearchFilters>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException(exceptionMessage));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("User search failed");
        result.Error.Should().Contain(exceptionMessage);
    }

    [Theory]
    [InlineData("john")]
    [InlineData("smith")]
    [InlineData("john.doe@example.com")]
    [InlineData("doe")]
    public async Task Handle_WithDifferentSearchTerms_ShouldSearchCorrectly(string searchTerm)
    {
        // Arrange
        var query = new SearchUsersQuery(searchTerm, new SearchFilters(), 1, 10);
        var expectedResult = Result.Success(new List<UserSearchResultDto>());

        _searchServiceMock
            .Setup(x => x.SearchUsersAsync(
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
            x => x.SearchUsersAsync(
                searchTerm,
                It.IsAny<SearchFilters>(),
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithFullNameMatches_ShouldReturnCorrectResults()
    {
        // Arrange
        var query = new SearchUsersQuery("John Doe", new SearchFilters(), 1, 10);
        var expectedUsers = new List<UserSearchResultDto>
        {
            new UserSearchResultDto
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@company.com",
                CreatedDate = DateTime.UtcNow.AddDays(-60)
            }
        };
        var expectedResult = Result.Success(expectedUsers);

        _searchServiceMock
            .Setup(x => x.SearchUsersAsync(
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
        result.Value.Should().HaveCount(1);
        result.Value[0].FullName.Should().Be("John Doe");
        result.Value[0].Email.Should().Be("john.doe@company.com");
    }

    [Fact]
    public async Task Handle_WithActiveAndInactiveUsers_ShouldReturnBothWhenFiltersAllowIt()
    {
        // Arrange
        var filters = new SearchFilters { ActiveOnly = false };
        var query = new SearchUsersQuery("user", filters, 1, 10);
        var expectedUsers = new List<UserSearchResultDto>
        {
            new UserSearchResultDto
            {
                Id = 1,
                FirstName = "Active",
                LastName = "User",
                Email = "active.user@example.com",
                CreatedDate = DateTime.UtcNow.AddDays(-10)
            },
            new UserSearchResultDto
            {
                Id = 1,
                FirstName = "Inactive",
                LastName = "User",
                Email = "inactive.user@example.com",
                CreatedDate = DateTime.UtcNow.AddDays(-20)
            }
        };
        var expectedResult = Result.Success(expectedUsers);

        _searchServiceMock
            .Setup(x => x.SearchUsersAsync(
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
        result.Value.Should().Contain(u => u.FirstName == "Active");
        result.Value.Should().Contain(u => u.FirstName == "Inactive");

        _searchServiceMock.Verify(
            x => x.SearchUsersAsync(
                "user",
                It.Is<SearchFilters>(f => f.ActiveOnly == false),
                1,
                10,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
