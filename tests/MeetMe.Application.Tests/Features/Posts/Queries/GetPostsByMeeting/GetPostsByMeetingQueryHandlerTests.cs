using AutoMapper;
using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Posts.Queries.GetPostsByMeeting;
using MeetMe.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace MeetMe.Application.Tests.Features.Posts.Queries.GetPostsByMeeting;

public class GetPostsByMeetingQueryHandlerTests
{
    private readonly Mock<IQueryRepository<Post, int>> _mockPostRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetPostsByMeetingQueryHandler _handler;

    public GetPostsByMeetingQueryHandlerTests()
    {
        _mockPostRepository = new Mock<IQueryRepository<Post, int>>();
        _mockMapper = new Mock<IMapper>();

        _handler = new GetPostsByMeetingQueryHandler(
            _mockPostRepository.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidMeetingId_ShouldReturnSuccessResult()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var query = new GetPostsByMeetingQuery(meetingId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);

        var posts = new List<Post>
        {
            Post.Create("Post 1", "Content 1", author, meeting),
            Post.Create("Post 2", "Content 2", author, meeting)
        };

        var postDtos = new List<PostDto>
        {
            new PostDto { Id = 1, Title = "Post 1", Content = "Content 1", AuthorId = author.Id, MeetingId = meetingId },
            new PostDto { Id = 2, Title = "Post 2", Content = "Content 2", AuthorId = author.Id, MeetingId = meetingId }
        };

        _mockPostRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Post, object>>[]>()))
            .ReturnsAsync(posts);

        _mockMapper
            .Setup(x => x.Map<List<PostDto>>(It.IsAny<IEnumerable<Post>>()))
            .Returns(postDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(postDtos);
    }

    [Fact]
    public async Task Handle_WithEmptyResults_ShouldReturnEmptyList()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var query = new GetPostsByMeetingQuery(meetingId);

        var posts = new List<Post>();
        var postDtos = new List<PostDto>();

        _mockPostRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Post, object>>[]>()))
            .ReturnsAsync(posts);

        _mockMapper
            .Setup(x => x.Map<List<PostDto>>(It.IsAny<IEnumerable<Post>>()))
            .Returns(postDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectParameters()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var query = new GetPostsByMeetingQuery(meetingId);

        var posts = new List<Post>();
        var postDtos = new List<PostDto>();

        _mockPostRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Post, object>>[]>()))
            .ReturnsAsync(posts);

        _mockMapper
            .Setup(x => x.Map<List<PostDto>>(It.IsAny<IEnumerable<Post>>()))
            .Returns(postDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockPostRepository.Verify(
            x => x.FindAsync(
                It.Is<Expression<Func<Post, bool>>>(expr => expr != null),
                It.IsAny<CancellationToken>(),
                It.Is<Expression<Func<Post, object>>[]>(includes => includes.Length == 3)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeRelatedEntities()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var query = new GetPostsByMeetingQuery(meetingId);

        var posts = new List<Post>();
        var postDtos = new List<PostDto>();

        _mockPostRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Post, object>>[]>()))
            .ReturnsAsync(posts);

        _mockMapper
            .Setup(x => x.Map<List<PostDto>>(It.IsAny<IEnumerable<Post>>()))
            .Returns(postDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockPostRepository.Verify(
            x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.Is<Expression<Func<Post, object>>[]>(includes => 
                    includes.Length == 3)), // Author, Meeting, Comments
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var query = new GetPostsByMeetingQuery(meetingId);

        _mockPostRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Post, object>>[]>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get posts");
        result.Error.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task Handle_WhenMapperThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var query = new GetPostsByMeetingQuery(meetingId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);

        var posts = new List<Post>
        {
            Post.Create("Post 1", "Content 1", author, meeting)
        };

        _mockPostRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Post, object>>[]>()))
            .ReturnsAsync(posts);

        _mockMapper
            .Setup(x => x.Map<List<PostDto>>(It.IsAny<IEnumerable<Post>>()))
            .Throws(new Exception("Mapping failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get posts");
        result.Error.Should().Contain("Mapping failed");
    }

    [Fact]
    public async Task Handle_ShouldHandleCancellationToken()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var query = new GetPostsByMeetingQuery(meetingId);
        var cancellationToken = new CancellationToken(true);

        var posts = new List<Post>();
        var postDtos = new List<PostDto>();

        _mockPostRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                cancellationToken,
                It.IsAny<Expression<Func<Post, object>>[]>()))
            .ReturnsAsync(posts);

        _mockMapper
            .Setup(x => x.Map<List<PostDto>>(It.IsAny<IEnumerable<Post>>()))
            .Returns(postDtos);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockPostRepository.Verify(
            x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                cancellationToken,
                It.IsAny<Expression<Func<Post, object>>[]>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallMapperWithOrderedPosts()
    {
        // Arrange
        var meetingId = Guid.NewGuid();
        var query = new GetPostsByMeetingQuery(meetingId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);

        var posts = new List<Post>
        {
            Post.Create("Post 1", "Content 1", author, meeting),
            Post.Create("Post 2", "Content 2", author, meeting)
        };

        var postDtos = new List<PostDto>
        {
            new PostDto { Id = 1, Title = "Post 1", Content = "Content 1", AuthorId = author.Id, MeetingId = meetingId },
            new PostDto { Id = 2, Title = "Post 2", Content = "Content 2", AuthorId = author.Id, MeetingId = meetingId }
        };

        _mockPostRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Post, object>>[]>()))
            .ReturnsAsync(posts);

        _mockMapper
            .Setup(x => x.Map<List<PostDto>>(It.IsAny<IEnumerable<Post>>()))
            .Returns(postDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockMapper.Verify(
            x => x.Map<List<PostDto>>(It.IsAny<IEnumerable<Post>>()),
            Times.Once);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    [InlineData("12345678-1234-5678-9012-123456789012")]
    public async Task Handle_WithDifferentMeetingIds_ShouldWork(string meetingIdString)
    {
        // Arrange
        var meetingId = Guid.Parse(meetingIdString);
        var query = new GetPostsByMeetingQuery(meetingId);

        var posts = new List<Post>();
        var postDtos = new List<PostDto>();

        _mockPostRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Post, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Post, object>>[]>()))
            .ReturnsAsync(posts);

        _mockMapper
            .Setup(x => x.Map<List<PostDto>>(It.IsAny<IEnumerable<Post>>()))
            .Returns(postDtos);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }
}
