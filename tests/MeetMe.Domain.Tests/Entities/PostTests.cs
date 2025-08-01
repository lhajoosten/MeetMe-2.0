using FluentAssertions;
using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;
using MeetMe.Domain.Events;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Domain.Tests.Entities;

public class PostTests
{
    private readonly User _author;
    private readonly Meeting _meeting;

    public PostTests()
    {
        _author = User.Create("John", "Doe", "john@example.com");
        _meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Conference Room",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            _author,
            10
        );
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreatePost()
    {
        // Arrange
        var title = "Test Post Title";
        var content = "Test post content";

        // Act
        var post = Post.Create(title, content, _author, _meeting);

        // Assert
        post.Should().NotBeNull();
        post.Title.Should().Be(title);
        post.Content.Should().Be(content);
        post.Author.Should().Be(_author);
        post.Meeting.Should().Be(_meeting);
        post.IsActive.Should().BeTrue();
        post.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<PostCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    public void Create_WithNullOrEmptyTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Arrange
        var content = "Test content";

        // Act & Assert
        var action = () => Post.Create(invalidTitle, content, _author, _meeting);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be null or empty.*");
    }

    [Fact]
    public void Create_WithNullTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var content = "Test content";

        // Act & Assert
        var action = () => Post.Create(null!, content, _author, _meeting);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be null or empty.*");
    }

    [Theory]
    [InlineData("")]
    public void Create_WithNullOrEmptyContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Arrange
        var title = "Test Title";

        // Act & Assert
        var action = () => Post.Create(title, invalidContent, _author, _meeting);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or empty.*");
    }

    [Fact]
    public void Create_WithNullContent_ShouldThrowArgumentException()
    {
        // Arrange
        var title = "Test Title";

        // Act & Assert
        var action = () => Post.Create(title, null!, _author, _meeting);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or empty.*");
    }

    [Fact]
    public void Create_WithNullAuthor_ShouldThrowArgumentNullException()
    {
        // Arrange
        var title = "Test Title";
        var content = "Test content";

        // Act & Assert
        var action = () => Post.Create(title, content, null!, _meeting);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Author cannot be null when creating a post.*");
    }

    [Fact]
    public void Create_WithNullMeeting_ShouldThrowArgumentNullException()
    {
        // Arrange
        var title = "Test Title";
        var content = "Test content";

        // Act & Assert
        var action = () => Post.Create(title, content, _author, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Meeting cannot be null when creating a post.*");
    }

    [Fact]
    public void UpdateContent_WithValidParameters_ShouldUpdatePostContent()
    {
        // Arrange
        var post = Post.Create("Original Title", "Original content", _author, _meeting);
        var newTitle = "Updated Title";
        var newContent = "Updated content";
        var updatingUser = User.Create("Jane", "Smith", "jane@example.com");

        // Act
        post.UpdateContent(newTitle, newContent, updatingUser);

        // Assert
        post.Title.Should().Be(newTitle);
        post.Content.Should().Be(newContent);
        post.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        post.LastModifiedByUserId.Should().Be(updatingUser.Id.ToString());
    }

    [Theory]
    [InlineData("")]
    public void UpdateContent_WithNullOrEmptyTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Arrange
        var post = Post.Create("Original Title", "Original content", _author, _meeting);
        var newContent = "Updated content";
        var updatingUser = User.Create("Jane", "Smith", "jane@example.com");

        // Act & Assert
        var action = () => post.UpdateContent(invalidTitle, newContent, updatingUser);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be null or empty.*");
    }

    [Fact]
    public void UpdateContent_WithNullTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var post = Post.Create("Original Title", "Original content", _author, _meeting);
        var newContent = "Updated content";
        var updatingUser = User.Create("Jane", "Smith", "jane@example.com");

        // Act & Assert
        var action = () => post.UpdateContent(null!, newContent, updatingUser);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be null or empty.*");
    }

    [Theory]
    [InlineData("")]
    public void UpdateContent_WithNullOrEmptyContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Arrange
        var post = Post.Create("Original Title", "Original content", _author, _meeting);
        var newTitle = "Updated Title";
        var updatingUser = User.Create("Jane", "Smith", "jane@example.com");

        // Act & Assert
        var action = () => post.UpdateContent(newTitle, invalidContent, updatingUser);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or empty.*");
    }

    [Fact]
    public void UpdateContent_WithNullContent_ShouldThrowArgumentException()
    {
        // Arrange
        var post = Post.Create("Original Title", "Original content", _author, _meeting);
        var newTitle = "Updated Title";
        var updatingUser = User.Create("Jane", "Smith", "jane@example.com");

        // Act & Assert
        var action = () => post.UpdateContent(newTitle, null!, updatingUser);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or empty.*");
    }

    [Fact]
    public void UpdateContent_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var post = Post.Create("Original Title", "Original content", _author, _meeting);
        var newTitle = "Updated Title";
        var newContent = "Updated content";

        // Act & Assert
        var action = () => post.UpdateContent(newTitle, newContent, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("User cannot be null when updating a post.*");
    }

    [Fact]
    public void Deactivate_WithValidUser_ShouldDeactivatePost()
    {
        // Arrange
        var post = Post.Create("Test Title", "Test content", _author, _meeting);
        var deactivatingUser = User.Create("Admin", "User", "admin@example.com");

        // Act
        post.Deactivate(deactivatingUser);

        // Assert
        post.IsActive.Should().BeFalse();
        post.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        post.LastModifiedByUserId.Should().Be(deactivatingUser.Id.ToString());
    }

    [Fact]
    public void Deactivate_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var post = Post.Create("Test Title", "Test content", _author, _meeting);

        // Act & Assert
        var action = () => post.Deactivate(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("User cannot be null when deactivating a post.*");
    }

    [Fact]
    public void ActiveCommentsCount_WithNoComments_ShouldReturnZero()
    {
        // Arrange
        var post = Post.Create("Test Title", "Test content", _author, _meeting);

        // Act
        var count = post.ActiveCommentsCount;

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToDomainEvents()
    {
        // Arrange
        var post = Post.Create("Test Title", "Test content", _author, _meeting);
        var customEvent = new PostCreatedEvent(post);

        // Act
        post.AddDomainEvent(customEvent);

        // Assert
        post.DomainEvents.Should().Contain(customEvent);
    }

    [Fact]
    public void RemoveDomainEvent_ShouldRemoveEventFromDomainEvents()
    {
        // Arrange
        var post = Post.Create("Test Title", "Test content", _author, _meeting);
        var eventToRemove = post.DomainEvents.First();

        // Act
        post.RemoveDomainEvent(eventToRemove);

        // Assert
        post.DomainEvents.Should().NotContain(eventToRemove);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllDomainEvents()
    {
        // Arrange
        var post = Post.Create("Test Title", "Test content", _author, _meeting);
        var customEvent = new PostCreatedEvent(post);
        post.AddDomainEvent(customEvent);

        // Act
        post.ClearDomainEvents();

        // Assert
        post.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        // Arrange
        var post = Post.Create("Test Title", "Test content", _author, _meeting);

        // Act
        var domainEvents = post.DomainEvents;

        // Assert
        domainEvents.Should().BeAssignableTo<IReadOnlyCollection<IDomainEvent>>();
    }

    [Fact]
    public void Create_ShouldSetAuthorIdAndMeetingIdFromEntities()
    {
        // Arrange
        var title = "Test Title";
        var content = "Test content";

        // Act
        var post = Post.Create(title, content, _author, _meeting);

        // Assert
        post.AuthorId.Should().Be(_author.Id);
        post.MeetingId.Should().Be(_meeting.Id);
    }
}
