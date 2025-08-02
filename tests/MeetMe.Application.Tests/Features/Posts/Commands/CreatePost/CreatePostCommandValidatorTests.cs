using FluentAssertions;
using MeetMe.Application.Features.Posts.Commands.CreatePost;

namespace MeetMe.Application.Tests.Features.Posts.Commands.CreatePost;

public class CreatePostCommandValidatorTests
{
    private readonly CreatePostCommandValidator _validator;

    public CreatePostCommandValidatorTests()
    {
        _validator = new CreatePostCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreatePostCommand(
            "Meeting Notes",
            "Here are the key points from today's meeting: 1. Project timeline updated 2. New requirements discussed",
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidTitle_ShouldFail(string title)
    {
        // Arrange
        var command = new CreatePostCommand(
            title,
            "Valid content for the post",
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Title));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title is required");
    }

    [Fact]
    public void Validate_WithTooLongTitle_ShouldFail()
    {
        // Arrange
        var longTitle = new string('A', 201); // 201 characters
        var command = new CreatePostCommand(
            longTitle,
            "Valid content for the post",
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Title));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_WithMaxLengthTitle_ShouldPass()
    {
        // Arrange
        var maxTitle = new string('A', 200); // Exactly 200 characters
        var command = new CreatePostCommand(
            maxTitle,
            "Valid content for the post",
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidContent_ShouldFail(string content)
    {
        // Arrange
        var command = new CreatePostCommand(
            "Valid Title",
            content,
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Content));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content is required");
    }

    [Fact]
    public void Validate_WithTooLongContent_ShouldFail()
    {
        // Arrange
        var longContent = new string('B', 5001); // 5001 characters
        var command = new CreatePostCommand(
            "Valid Title",
            longContent,
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Content));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content cannot exceed 5000 characters");
    }

    [Fact]
    public void Validate_WithMaxLengthContent_ShouldPass()
    {
        // Arrange
        var maxContent = new string('B', 5000); // Exactly 5000 characters
        var command = new CreatePostCommand(
            "Valid Title",
            maxContent,
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyMeetingId_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand(
            "Valid Title",
            "Valid content for the post",
            0,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.MeetingId));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Meeting ID is required");
    }

    [Fact]
    public void Validate_WithEmptyAuthorId_ShouldFail()
    {
        // Arrange
        var command = new CreatePostCommand(
            "Valid Title",
            "Valid content for the post",
            1,
            0);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.AuthorId));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Author ID is required");
    }

    [Fact]
    public void Validate_WithValidGuids_ShouldPass()
    {
        // Arrange
        var meetingId = 1;
        var authorId = 1;
        var command = new CreatePostCommand(
            "Valid Title",
            "Valid content for the post",
            meetingId,
            authorId);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new CreatePostCommand(
            "", // Invalid title
            "", // Invalid content
            0, // Invalid meeting ID
            0); // Invalid author ID

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Title));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.Content));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.MeetingId));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreatePostCommand.AuthorId));
    }

    [Theory]
    [InlineData("Short")]
    [InlineData("Medium length title for discussion")]
    [InlineData("Very long title that contains multiple words and describes the meeting topic in detail but stays within the 200 character limit set by the validation rules")]
    public void Validate_WithVariousValidTitles_ShouldPass(string title)
    {
        // Arrange
        var command = new CreatePostCommand(
            title,
            "Valid content for the post",
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Brief content")]
    [InlineData("This is a longer content that describes the meeting details, action items, and next steps in more detail.")]
    [InlineData("Comprehensive meeting summary with multiple paragraphs.\n\nKey Discussion Points:\n- Item 1\n- Item 2\n- Item 3\n\nAction Items:\n- Task 1 assigned to John\n- Task 2 assigned to Jane\n\nNext Meeting: Next Friday at 2 PM")]
    public void Validate_WithVariousValidContent_ShouldPass(string content)
    {
        // Arrange
        var command = new CreatePostCommand(
            "Valid Title",
            content,
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithSpecialCharactersInTitle_ShouldPass()
    {
        // Arrange
        var command = new CreatePostCommand(
            "Meeting Notes - Q3 Planning & Review (2024)",
            "Valid content for the post",
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithFormattedContent_ShouldPass()
    {
        // Arrange
        var formattedContent = @"**Meeting Summary**

        Attendees: 
        - John Doe
        - Jane Smith

        Topics Discussed:
        1. Project timeline
        2. Budget allocation
        3. Resource planning

        Action Items:
        • Review requirements - Due: Friday
        • Update documentation - Due: Next Monday";

        var command = new CreatePostCommand(
            "Weekly Team Meeting",
            formattedContent,
            1,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
