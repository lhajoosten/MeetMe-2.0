using FluentAssertions;
using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;
using MeetMe.Domain.Events;

namespace MeetMe.Domain.Tests.Entities;

public class CommentTests
{
    private readonly User _author;
    private readonly User _otherUser;
    private readonly Meeting _meeting;
    private readonly Post _post;

    public CommentTests()
    {
        _author = User.Create("John", "Doe", "john@example.com");
        _otherUser = User.Create("Jane", "Smith", "jane@example.com");
        _meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Conference Room",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            _author,
            10
        );
        _post = Post.Create("Test Post", "Test content", _author, _meeting);
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateComment()
    {
        // Arrange
        var content = "This is a test comment";

        // Act
        var comment = Comment.Create(content, _author, _post);

        // Assert
        comment.Should().NotBeNull();
        comment.Content.Should().Be(content);
        comment.Author.Should().Be(_author);
        comment.Post.Should().Be(_post);
        comment.ParentComment.Should().BeNull();
        comment.IsActive.Should().BeTrue();
        comment.IsReply.Should().BeFalse();
        comment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CommentCreatedEvent>();
    }

    [Fact]
    public void Create_WithParentComment_ShouldCreateReplyComment()
    {
        // Arrange
        var parentComment = Comment.Create("Parent comment", _author, _post);
        var replyContent = "This is a reply";

        // Act
        var replyComment = Comment.Create(replyContent, _otherUser, _post, parentComment);

        // Assert
        replyComment.Should().NotBeNull();
        replyComment.Content.Should().Be(replyContent);
        replyComment.Author.Should().Be(_otherUser);
        replyComment.Post.Should().Be(_post);
        replyComment.ParentComment.Should().Be(parentComment);
        replyComment.IsReply.Should().BeTrue();
        replyComment.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CommentCreatedEvent>();
    }

    [Theory]
    [InlineData("")]
    public void Create_WithNullOrEmptyContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Act & Assert
        var action = () => Comment.Create(invalidContent, _author, _post);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or empty.*");
    }

    [Fact]
    public void Create_WithNullContent_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => Comment.Create(null!, _author, _post);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or empty.*");
    }

    [Fact]
    public void Create_WithNullAuthor_ShouldThrowArgumentNullException()
    {
        // Arrange
        var content = "Test content";

        // Act & Assert
        var action = () => Comment.Create(content, null!, _post);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Author cannot be null when creating a comment.*");
    }

    [Fact]
    public void Create_WithNullPost_ShouldThrowArgumentNullException()
    {
        // Arrange
        var content = "Test content";

        // Act & Assert
        var action = () => Comment.Create(content, _author, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Post cannot be null when creating a comment.*");
    }

    [Fact]
    public void UpdateContent_WithValidParameters_ShouldUpdateCommentContent()
    {
        // Arrange
        var comment = Comment.Create("Original content", _author, _post);
        var newContent = "Updated content";
        var updatingUser = User.Create("Admin", "User", "admin@example.com");

        // Act
        comment.UpdateContent(newContent, updatingUser);

        // Assert
        comment.Content.Should().Be(newContent);
        comment.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        comment.LastModifiedByUserId.Should().Be(updatingUser.Id);
    }

    [Theory]
    [InlineData("")]
    public void UpdateContent_WithNullOrEmptyContent_ShouldThrowArgumentException(string invalidContent)
    {
        // Arrange
        var comment = Comment.Create("Original content", _author, _post);
        var updatingUser = User.Create("Admin", "User", "admin@example.com");

        // Act & Assert
        var action = () => comment.UpdateContent(invalidContent, updatingUser);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or empty.*");
    }

    [Fact]
    public void UpdateContent_WithNullContent_ShouldThrowArgumentException()
    {
        // Arrange
        var comment = Comment.Create("Original content", _author, _post);
        var updatingUser = User.Create("Admin", "User", "admin@example.com");

        // Act & Assert
        var action = () => comment.UpdateContent(null!, updatingUser);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Content cannot be null or empty.*");
    }

    [Fact]
    public void UpdateContent_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var comment = Comment.Create("Original content", _author, _post);
        var newContent = "Updated content";

        // Act & Assert
        var action = () => comment.UpdateContent(newContent, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("User cannot be null when updating a comment.*");
    }

    [Fact]
    public void Deactivate_WithValidUser_ShouldDeactivateComment()
    {
        // Arrange
        var comment = Comment.Create("Test content", _author, _post);
        var deactivatingUser = User.Create("Admin", "User", "admin@example.com");

        // Act
        comment.Deactivate(deactivatingUser);

        // Assert
        comment.IsActive.Should().BeFalse();
        comment.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        comment.LastModifiedByUserId.Should().Be(deactivatingUser.Id);
    }

    [Fact]
    public void Deactivate_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var comment = Comment.Create("Test content", _author, _post);

        // Act & Assert
        var action = () => comment.Deactivate(null!);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("User cannot be null when deactivating a comment.*");
    }

    [Fact]
    public void ActiveRepliesCount_WithActiveAndInactiveReplies_ShouldReturnCorrectCount()
    {
        // Arrange
        var parentComment = Comment.Create("Parent comment", _author, _post);
        var reply1 = Comment.Create("Reply 1", _otherUser, _post, parentComment);
        var reply2 = Comment.Create("Reply 2", _author, _post, parentComment);
        var reply3 = Comment.Create("Reply 3", _otherUser, _post, parentComment);

        // Deactivate one reply
        reply2.Deactivate(_author);

        // Manually add replies to the collection for testing
        parentComment.Replies.Add(reply1);
        parentComment.Replies.Add(reply2);
        parentComment.Replies.Add(reply3);

        // Act
        var activeRepliesCount = parentComment.ActiveRepliesCount;

        // Assert
        activeRepliesCount.Should().Be(2); // Only reply1 and reply3 are active
    }

    [Fact]
    public void IsReply_WithParentComment_ShouldReturnTrue()
    {
        // Arrange
        var parentComment = Comment.Create("Parent comment", _author, _post);
        var replyComment = Comment.Create("Reply comment", _otherUser, _post, parentComment);

        // Act & Assert
        replyComment.IsReply.Should().BeTrue();
    }

    [Fact]
    public void IsReply_WithoutParentComment_ShouldReturnFalse()
    {
        // Arrange
        var comment = Comment.Create("Regular comment", _author, _post);

        // Act & Assert
        comment.IsReply.Should().BeFalse();
    }

    [Fact]
    public void DomainEvents_Management_ShouldWorkCorrectly()
    {
        // Arrange
        var comment = Comment.Create("Test content", _author, _post);
        var initialEvent = comment.DomainEvents.First();

        // Act & Assert - Initial state
        comment.DomainEvents.Should().ContainSingle();

        // Act - Remove domain event
        comment.RemoveDomainEvent(initialEvent);

        // Assert
        comment.DomainEvents.Should().BeEmpty();

        // Act - Add domain event back
        comment.AddDomainEvent(initialEvent);

        // Assert
        comment.DomainEvents.Should().ContainSingle();

        // Act - Clear all domain events
        comment.ClearDomainEvents();

        // Assert
        comment.DomainEvents.Should().BeEmpty();
    }
}
