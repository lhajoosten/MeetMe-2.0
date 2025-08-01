using AutoMapper;
using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Posts.Commands.UpdatePost;
using MeetMe.Domain.Entities;
using Moq;

namespace MeetMe.Application.Tests.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandHandlerTests
{
    private readonly Mock<ICommandRepository<Post, int>> _mockPostCommandRepository;
    private readonly Mock<IQueryRepository<Post, int>> _mockPostQueryRepository;
    private readonly Mock<IQueryRepository<User, Guid>> _mockUserQueryRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly UpdatePostCommandHandler _handler;

    public UpdatePostCommandHandlerTests()
    {
        _mockPostCommandRepository = new Mock<ICommandRepository<Post, int>>();
        _mockPostQueryRepository = new Mock<IQueryRepository<Post, int>>();
        _mockUserQueryRepository = new Mock<IQueryRepository<User, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();

        _handler = new UpdatePostCommandHandler(
            _mockPostCommandRepository.Object,
            _mockPostQueryRepository.Object,
            _mockUserQueryRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var postId = 1;
        var userId = Guid.NewGuid();
        var command = new UpdatePostCommand(postId, "Updated Title", "Updated Content", userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Original Title", "Original Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        var postDto = new PostDto
        {
            Id = postId,
            Title = "Updated Title",
            Content = "Updated Content",
            AuthorId = userId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockPostCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockMapper
            .Setup(x => x.Map<PostDto>(It.IsAny<Post>()))
            .Returns(postDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be("Updated Title");
        result.Value.Content.Should().Be("Updated Content");
        result.Value.AuthorId.Should().Be(userId);

        _mockPostCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Post>(), userId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(userId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 999;
        var userId = Guid.NewGuid();
        var command = new UpdatePostCommand(postId, "Updated Title", "Updated Content", userId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Post?)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Post not found");

        _mockUserQueryRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _mockPostCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 1;
        var userId = Guid.NewGuid();
        var command = new UpdatePostCommand(postId, "Updated Title", "Updated Content", userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Original Title", "Original Content", author, meeting);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");

        _mockPostCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithUnauthorizedUser_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 1;
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var command = new UpdatePostCommand(postId, "Updated Title", "Updated Content", userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Original Title", "Original Content", author, meeting);
        
        // Set the author ID to a different user
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, differentUserId);

        var requestingUser = User.Create("Jane", "Smith", "jane.smith@example.com");

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(requestingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("You don't have permission to update this post");

        _mockPostCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 1;
        var userId = Guid.NewGuid();
        var command = new UpdatePostCommand(postId, "Updated Title", "Updated Content", userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Original Title", "Original Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockPostCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update post");
        result.Error.Should().Contain("Database error");

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var postId = 1;
        var userId = Guid.NewGuid();
        var command = new UpdatePostCommand(postId, "Updated Title", "Updated Content", userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Original Title", "Original Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockPostCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save changes failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update post");
        result.Error.Should().Contain("Save changes failed");
    }

    [Fact]
    public async Task Handle_ShouldCallMapperWithUpdatedPost()
    {
        // Arrange
        var postId = 1;
        var userId = Guid.NewGuid();
        var command = new UpdatePostCommand(postId, "Updated Title", "Updated Content", userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Original Title", "Original Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        // Set the post ID to match the expected ID
        var idProperty = postType.GetProperty("Id");
        idProperty?.SetValue(post, postId);

        var postDto = new PostDto
        {
            Id = postId,
            Title = "Updated Title",
            Content = "Updated Content",
            AuthorId = userId
        };

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockPostCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockMapper
            .Setup(x => x.Map<PostDto>(It.IsAny<Post>()))
            .Returns(postDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockMapper.Verify(
            x => x.Map<PostDto>(It.Is<Post>(p => p.Id == postId)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserIdToRepositoryAndUnitOfWork()
    {
        // Arrange
        var postId = 1;
        var userId = Guid.NewGuid();
        var command = new UpdatePostCommand(postId, "Updated Title", "Updated Content", userId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Test Location",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(2),
            author);
        var post = Post.Create("Original Title", "Original Content", author, meeting);
        
        // Set the author ID to match the command user ID
        var postType = typeof(Post);
        var authorIdProperty = postType.GetProperty("AuthorId");
        authorIdProperty?.SetValue(post, userId);

        var postDto = new PostDto();

        _mockPostQueryRepository
            .Setup(x => x.GetByIdAsync(postId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(post);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockPostCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockMapper
            .Setup(x => x.Map<PostDto>(It.IsAny<Post>()))
            .Returns(postDto);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockPostCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Post>(), userId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(userId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
