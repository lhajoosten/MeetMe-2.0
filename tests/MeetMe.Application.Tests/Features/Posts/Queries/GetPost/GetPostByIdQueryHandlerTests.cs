using AutoMapper;
using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Posts.Queries.GetPost;
using MeetMe.Domain.Entities;
using Moq;

namespace MeetMe.Application.Tests.Features.Posts.Queries.GetPost;

public class GetPostByIdQueryHandlerTests
{
    private readonly Mock<IQueryRepository<Post, int>> _mockPostRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetPostByIdQueryHandler _handler;

    public GetPostByIdQueryHandlerTests()
    {
        _mockPostRepository = new Mock<IQueryRepository<Post, int>>();
        _mockMapper = new Mock<IMapper>();

        _handler = new GetPostByIdQueryHandler(_mockPostRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnSuccessResult()
    {
        // Arrange
        var postId = 1;
        var query = new GetPostByIdQuery(postId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);

        var postDto = new PostDto
        {
            Id = postId,
            Title = "Test Title",
            Content = "Test Content",
            AuthorId = author.Id,
            AuthorName = "John Doe",
            MeetingId = meeting.Id,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _mockPostRepository
            .Setup(x => x.GetByIdAsync(
                postId, 
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()))
            .ReturnsAsync(post);

        _mockMapper
            .Setup(x => x.Map<PostDto>(post))
            .Returns(postDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(postId);
        result.Value.Title.Should().Be("Test Title");
        result.Value.Content.Should().Be("Test Content");
        result.Value.AuthorName.Should().Be("John Doe");

        _mockPostRepository.Verify(
            x => x.GetByIdAsync(
                postId, 
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()),
            Times.Once);

        _mockMapper.Verify(x => x.Map<PostDto>(post), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 999;
        var query = new GetPostByIdQuery(postId);

        _mockPostRepository
            .Setup(x => x.GetByIdAsync(
                postId, 
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()))
            .ReturnsAsync((Post?)null!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Post not found");

        _mockMapper.Verify(x => x.Map<PostDto>(It.IsAny<Post>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 1;
        var query = new GetPostByIdQuery(postId);

        _mockPostRepository
            .Setup(x => x.GetByIdAsync(
                postId, 
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get post");
        result.Error.Should().Contain("Database connection failed");

        _mockMapper.Verify(x => x.Map<PostDto>(It.IsAny<Post>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMapperThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 1;
        var query = new GetPostByIdQuery(postId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);

        _mockPostRepository
            .Setup(x => x.GetByIdAsync(
                postId, 
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()))
            .ReturnsAsync(post);

        _mockMapper
            .Setup(x => x.Map<PostDto>(post))
            .Throws(new Exception("Mapping failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get post");
        result.Error.Should().Contain("Mapping failed");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    [InlineData(999)]
    public async Task Handle_WithDifferentValidIds_ShouldCallRepositoryWithCorrectId(int postId)
    {
        // Arrange
        var query = new GetPostByIdQuery(postId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);

        var postDto = new PostDto { Id = postId, Title = "Test Title", Content = "Test Content" };

        _mockPostRepository
            .Setup(x => x.GetByIdAsync(
                postId, 
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()))
            .ReturnsAsync(post);

        _mockMapper
            .Setup(x => x.Map<PostDto>(post))
            .Returns(postDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockPostRepository.Verify(
            x => x.GetByIdAsync(
                postId, 
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldIncludeRelatedEntities()
    {
        // Arrange
        var postId = 1;
        var query = new GetPostByIdQuery(postId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);

        var postDto = new PostDto { Id = postId, Title = "Test Title", Content = "Test Content" };

        _mockPostRepository
            .Setup(x => x.GetByIdAsync(
                postId, 
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()))
            .ReturnsAsync(post);

        _mockMapper
            .Setup(x => x.Map<PostDto>(post))
            .Returns(postDto);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockPostRepository.Verify(
            x => x.GetByIdAsync(
                postId, 
                It.IsAny<CancellationToken>(),
                It.IsNotNull<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var postId = 1;
        var query = new GetPostByIdQuery(postId);
        var cancellationToken = new CancellationToken();

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);

        var postDto = new PostDto { Id = postId, Title = "Test Title", Content = "Test Content" };

        _mockPostRepository
            .Setup(x => x.GetByIdAsync(
                postId, 
                cancellationToken,
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()))
            .ReturnsAsync(post);

        _mockMapper
            .Setup(x => x.Map<PostDto>(post))
            .Returns(postDto);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        _mockPostRepository.Verify(
            x => x.GetByIdAsync(
                postId, 
                cancellationToken,
                It.IsAny<System.Linq.Expressions.Expression<System.Func<Post, object>>[]>()),
            Times.Once);
    }
}
