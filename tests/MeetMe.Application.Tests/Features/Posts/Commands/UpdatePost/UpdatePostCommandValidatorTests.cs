using FluentAssertions;
using MeetMe.Application.Features.Posts.Commands.UpdatePost;

namespace MeetMe.Application.Tests.Features.Posts.Commands.UpdatePost;

public class UpdatePostCommandValidatorTests
{
    private readonly UpdatePostCommandValidator _validator;

    public UpdatePostCommandValidatorTests()
    {
        _validator = new UpdatePostCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new UpdatePostCommand(
            1,
            "Updated Meeting Notes",
            "Updated content with new information and action items",
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidId_ShouldFail(int id)
    {
        // Arrange
        var command = new UpdatePostCommand(
            id,
            "Valid Title",
            "Valid content for the post",
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Id));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Post ID must be greater than 0");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithInvalidTitle_ShouldFail(string title)
    {
        // Arrange
        var command = new UpdatePostCommand(
            1,
            title,
            "Valid content for the post",
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Title));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title is required");
    }

    [Fact]
    public void Validate_WithNullTitle_ShouldFail()
    {
        // Arrange
        var command = new UpdatePostCommand(
            1,
            null!,
            "Valid content for the post",
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Title));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title is required");
    }

    [Fact]
    public void Validate_WithTooLongTitle_ShouldFail()
    {
        // Arrange
        var longTitle = new string('A', 201); // 201 characters
        var command = new UpdatePostCommand(
            1,
            longTitle,
            "Valid content for the post",
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Title));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Title cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_WithMaxLengthTitle_ShouldPass()
    {
        // Arrange
        var maxTitle = new string('A', 200); // Exactly 200 characters
        var command = new UpdatePostCommand(
            1,
            maxTitle,
            "Valid content for the post",
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WithInvalidContent_ShouldFail(string content)
    {
        // Arrange
        var command = new UpdatePostCommand(
            1,
            "Valid Title",
            content,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Content));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content is required");
    }

    [Fact]
    public void Validate_WithNullContent_ShouldFail()
    {
        // Arrange
        var command = new UpdatePostCommand(
            1,
            "Valid Title",
            null!,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Content));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content is required");
    }

    [Fact]
    public void Validate_WithTooLongContent_ShouldFail()
    {
        // Arrange
        var longContent = new string('B', 5001); // 5001 characters
        var command = new UpdatePostCommand(
            1,
            "Valid Title",
            longContent,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Content));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Content cannot exceed 5000 characters");
    }

    [Fact]
    public void Validate_WithMaxLengthContent_ShouldPass()
    {
        // Arrange
        var maxContent = new string('B', 5000); // Exactly 5000 characters
        var command = new UpdatePostCommand(
            1,
            "Valid Title",
            maxContent,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var command = new UpdatePostCommand(
            1,
            "Valid Title",
            "Valid content for the post",
            0);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.UserId));
        result.Errors.Should().Contain(e => e.ErrorMessage == "User ID is required");
    }

    [Fact]
    public void Validate_WithValidUserId_ShouldPass()
    {
        // Arrange
        var userId = 1;
        var command = new UpdatePostCommand(
            1,
            "Valid Title",
            "Valid content for the post",
            userId);

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
        var command = new UpdatePostCommand(
            0, // Invalid ID
            "", // Invalid title
            "", // Invalid content
            0); // Invalid user ID

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Id));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Title));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.Content));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdatePostCommand.UserId));
    }

    [Theory]
    [InlineData(1, "Short")]
    [InlineData(50, "Medium length title for discussion")]
    [InlineData(999, "Very long title that contains multiple words and describes the meeting topic in detail but stays within the 200 character limit set by the validation rules")]
    public void Validate_WithVariousValidValues_ShouldPass(int id, string title)
    {
        // Arrange
        var command = new UpdatePostCommand(
            id,
            title,
            "Valid content for the post",
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Brief updated content")]
    [InlineData("This is a longer updated content that describes the meeting updates, revised action items, and next steps in more detail.")]
    [InlineData("Comprehensive meeting update with multiple sections.\n\nRevised Discussion Points:\n- Updated Item 1\n- Modified Item 2\n- New Item 3\n\nRevised Action Items:\n- Updated Task 1 assigned to John\n- Modified Task 2 assigned to Jane\n\nNext Meeting: Moved to Next Monday at 3 PM")]
    public void Validate_WithVariousValidContent_ShouldPass(string content)
    {
        // Arrange
        var command = new UpdatePostCommand(
            1,
            "Valid Title",
            content,
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
        var command = new UpdatePostCommand(
            1,
            "Updated Meeting Notes - Q3 Planning & Review (2024) - REVISED",
            "Valid content for the post",
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithFormattedUpdatedContent_ShouldPass()
    {
        // Arrange
        var formattedContent = @"**UPDATED Meeting Summary**

        Attendees: 
        - John Doe (added)
        - Jane Smith
        - Bob Wilson (removed)

        Revised Topics Discussed:
        1. Updated project timeline
        2. Revised budget allocation
        3. Modified resource planning

        Updated Action Items:
        • Review requirements - Due: Next Friday (extended)
        • Update documentation - Due: Following Monday
        • New task: Prepare presentation - Due: Wednesday";

        var command = new UpdatePostCommand(
            1,
            "Weekly Team Meeting - Updated",
            formattedContent,
            1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
