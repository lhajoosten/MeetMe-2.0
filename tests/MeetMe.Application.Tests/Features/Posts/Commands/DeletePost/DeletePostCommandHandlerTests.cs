using FluentAssertions;
using MeetMe.Application.Features.Posts.Commands.DeletePost;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Domain.Entities;
using Moq;

namespace MeetMe.Application.Tests.Features.Posts.Commands.DeletePost;

public class DeletePostCommandHandlerTests
{
    private readonly Mock<ICommandRepository<Post, int>> _mockPostCommandRepository;
    private readonly Mock<IQueryRepository<Post, int>> _mockPostQueryRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly DeletePostCommandHandler _handler;

    public DeletePostCommandHandlerTests()
    {
        _mockPostCommandRepository = new Mock<ICommandRepository<Post, int>>();
        _mockPostQueryRepository = new Mock<IQueryRepository<Post, int>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _handler = new DeletePostCommandHandler(
            _mockPostCommandRepository.Object,
            _mockPostQueryRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_WithValidPostAndAuthorizedUser_ShouldDeletePostAndReturnSuccess()
    {
        // Arrange
        var postId = 1;
        var userId = 1;
        var command = new DeletePostCommand(postId, userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        // Set the post ID to match the expected ID
        var idProperty = postType.GetProperty("Id");
        idProperty?.SetValue(post, postId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockPostCommandRepository
            .Setup(x => x.SoftDeleteAsync(It.IsAny<Post>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        _mockPostCommandRepository.Verify(
            x => x.SoftDeleteAsync(post, userId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 999;
        var userId = 1;
        var command = new DeletePostCommand(postId, userId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Post not found");

        _mockPostCommandRepository.Verify(
            x => x.SoftDeleteAsync(It.IsAny<Post>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 1;
        var userId = 1;
        var differentUserId = 2;
        var command = new DeletePostCommand(postId, differentUserId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);
        
        // Set the author ID to the original user (not the command user)
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        // Set the post ID
        var idProperty = postType.GetProperty("Id");
        idProperty?.SetValue(post, postId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You don't have permission to delete this post");

        _mockPostCommandRepository.Verify(
            x => x.SoftDeleteAsync(It.IsAny<Post>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSoftDeleteThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 1;
        var userId = 1;
        var command = new DeletePostCommand(postId, userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        // Set the post ID
        var idProperty = postType.GetProperty("Id");
        idProperty?.SetValue(post, postId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockPostCommandRepository
            .Setup(x => x.SoftDeleteAsync(It.IsAny<Post>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to delete post");
        result.Error.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 1;
        var userId = 1;
        var command = new DeletePostCommand(postId, userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        // Set the post ID
        var idProperty = postType.GetProperty("Id");
        idProperty?.SetValue(post, postId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockPostCommandRepository
            .Setup(x => x.SoftDeleteAsync(It.IsAny<Post>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save changes failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to delete post");
        result.Error.Should().Contain("Save changes failed");
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserIdToRepositoryAndUnitOfWork()
    {
        // Arrange
        var postId = 1;
        var userId = 1;
        var command = new DeletePostCommand(postId, userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        // Set the post ID
        var idProperty = postType.GetProperty("Id");
        idProperty?.SetValue(post, postId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockPostCommandRepository
            .Setup(x => x.SoftDeleteAsync(It.IsAny<Post>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockPostCommandRepository.Verify(
            x => x.SoftDeleteAsync(It.IsAny<Post>(), userId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleCancellationToken()
    {
        // Arrange
        var postId = 1;
        var userId = 1;
        var command = new DeletePostCommand(postId, userId);
        var cancellationToken = new CancellationToken(true);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Test Title", "Test Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        // Set the post ID
        var idProperty = postType.GetProperty("Id");
        idProperty?.SetValue(post, postId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, cancellationToken))
            .ReturnsAsync(post);

        _mockPostCommandRepository
            .Setup(x => x.SoftDeleteAsync(It.IsAny<Post>(), It.IsAny<int>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockPostQueryRepository.Verify(
            x => x.GetByIdAsync(postId, cancellationToken),
            Times.Once);

        _mockPostCommandRepository.Verify(
            x => x.SoftDeleteAsync(post, userId, cancellationToken),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(userId, cancellationToken),
            Times.Once);
    }
}
