using AutoMapper;
using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Posts.Commands.CreatePost;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;
using Moq;

namespace MeetMe.Application.Tests.Features.Posts.Commands.CreatePost;

public class CreatePostCommandHandlerTests
{
    private readonly Mock<ICommandRepository<Post, int>> _mockPostCommandRepository;
    private readonly Mock<IQueryRepository<User, Guid>> _mockUserQueryRepository;
    private readonly Mock<IQueryRepository<Meeting, Guid>> _mockMeetingQueryRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IMapper> _mockMapper;
    private readonly CreatePostCommandHandler _handler;

    public CreatePostCommandHandlerTests()
    {
        _mockPostCommandRepository = new Mock<ICommandRepository<Post, int>>();
        _mockUserQueryRepository = new Mock<IQueryRepository<User, Guid>>();
        _mockMeetingQueryRepository = new Mock<IQueryRepository<Meeting, Guid>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMapper = new Mock<IMapper>();

        _handler = new CreatePostCommandHandler(
            _mockPostCommandRepository.Object,
            _mockUserQueryRepository.Object,
            _mockMeetingQueryRepository.Object,
            _mockUnitOfWork.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();
        var command = new CreatePostCommand("Test Title", "Test Content", meetingId, authorId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting", 
            "Test Description", 
            "Test Location", 
            DateTime.Now.AddHours(1), 
            DateTime.Now.AddHours(2), 
            author);
        
        var createdPost = Post.Create("Test Title", "Test Content", author, meeting);
        var postDto = new PostDto
        {
            Id = 1,
            Title = "Test Title",
            Content = "Test Content",
            AuthorId = authorId,
            MeetingId = meetingId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _mockPostCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPost);

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
        result.Value.Title.Should().Be("Test Title");
        result.Value.Content.Should().Be("Test Content");
        result.Value.AuthorId.Should().Be(authorId);
        result.Value.MeetingId.Should().Be(meetingId);

        _mockPostCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<Post>(), authorId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(authorId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentAuthor_ShouldReturnFailureResult()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();
        var command = new CreatePostCommand("Test Title", "Test Content", meetingId, authorId);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Author not found");

        _mockPostCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentMeeting_ShouldReturnFailureResult()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();
        var command = new CreatePostCommand("Test Title", "Test Content", meetingId, authorId);

        var author = User.Create("John", "Doe", "john.doe@example.com");

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meeting?)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Meeting not found");

        _mockPostCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();
        var command = new CreatePostCommand("Test Title", "Test Content", meetingId, authorId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting", 
            "Test Description", 
            "Test Location", 
            DateTime.Now.AddHours(1), 
            DateTime.Now.AddHours(2), 
            author);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _mockPostCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create post");
        result.Error.Should().Contain("Database error");

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();
        var command = new CreatePostCommand("Test Title", "Test Content", meetingId, authorId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting", 
            "Test Description", 
            "Test Location", 
            DateTime.Now.AddHours(1), 
            DateTime.Now.AddHours(2), 
            author);
        var createdPost = Post.Create("Test Title", "Test Content", author, meeting);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _mockPostCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPost);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save changes failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create post");
        result.Error.Should().Contain("Save changes failed");
    }

    [Fact]
    public async Task Handle_ShouldCallMapperWithCreatedPost()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();
        var command = new CreatePostCommand("Test Title", "Test Content", meetingId, authorId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting", 
            "Test Description", 
            "Test Location", 
            DateTime.Now.AddHours(1), 
            DateTime.Now.AddHours(2), 
            author);
        var createdPost = Post.Create("Test Title", "Test Content", author, meeting);
        var postDto = new PostDto
        {
            Id = 1,
            Title = "Test Title",
            Content = "Test Content",
            AuthorId = authorId,
            MeetingId = meetingId
        };

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _mockPostCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPost);

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
            x => x.Map<PostDto>(It.Is<Post>(p => 
                p.Title == "Test Title" && 
                p.Content == "Test Content")),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldPassCorrectUserIdToRepositoryAndUnitOfWork()
    {
        // Arrange
        var authorId = Guid.NewGuid();
        var meetingId = Guid.NewGuid();
        var command = new CreatePostCommand("Test Title", "Test Content", meetingId, authorId);

        var author = User.Create("John", "Doe", "john.doe@example.com");
        var meeting = Meeting.Create(
            "Test Meeting", 
            "Test Description", 
            "Test Location", 
            DateTime.Now.AddHours(1), 
            DateTime.Now.AddHours(2), 
            author);
        var createdPost = Post.Create("Test Title", "Test Content", author, meeting);
        var postDto = new PostDto();

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(authorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(author);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        _mockPostCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<Post>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPost);

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
            x => x.AddAsync(It.IsAny<Post>(), authorId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
        
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(authorId.ToString(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
