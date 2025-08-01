using FluentAssertions;
using FluentValidation.TestHelper;
using MeetMe.Application.Features.Meetings.Commands.CreateMeeting;

namespace MeetMe.Application.Tests.Features.Meetings.Commands;

public class CreateMeetingCommandValidatorTests
{
    private readonly CreateMeetingCommandValidator _validator;

    public CreateMeetingCommandValidatorTests()
    {
        _validator = new CreateMeetingCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var command = new CreateMeetingCommand
        {
            Title = "Valid Meeting Title",
            Description = "Valid meeting description",
            Location = "Conference Room A",
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxAttendees = 10,
            CreatorId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidTitle_ShouldHaveValidationError(string invalidTitle)
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { Title = invalidTitle };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_WithTitleTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { Title = new string('a', 201) }; // Exceeds 200 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title cannot exceed 200 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidDescription_ShouldHaveValidationError(string invalidDescription)
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { Description = invalidDescription };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description is required");
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { Description = new string('a', 2001) }; // Exceeds 2000 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 2000 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidLocation_ShouldHaveValidationError(string invalidLocation)
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { Location = invalidLocation };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location is required");
    }

    [Fact]
    public void Validate_WithLocationTooLong_ShouldHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { Location = new string('a', 501) }; // Exceeds 500 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location cannot exceed 500 characters");
    }

    [Fact]
    public void Validate_WithPastStartDateTime_ShouldHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { StartDateTime = DateTime.UtcNow.AddHours(-1) }; // Past date

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDateTime)
            .WithErrorMessage("Meeting cannot be scheduled in the past");
    }

    [Fact]
    public void Validate_WithEndDateTimeBeforeStartDateTime_ShouldHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        var startTime = DateTime.UtcNow.AddDays(1);
        command = command with 
        { 
            StartDateTime = startTime,
            EndDateTime = startTime.AddHours(-1) // End before start
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDateTime)
            .WithErrorMessage("End date must be after start date");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_WithInvalidMaxAttendees_ShouldHaveValidationError(int invalidMaxAttendees)
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { MaxAttendees = invalidMaxAttendees };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxAttendees)
            .WithErrorMessage("Maximum attendees must be greater than 0");
    }

    [Fact]
    public void Validate_WithNullMaxAttendees_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { MaxAttendees = null };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxAttendees);
    }

    [Fact]
    public void Validate_WithEmptyCreatorId_ShouldHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { CreatorId = Guid.Empty };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CreatorId)
            .WithErrorMessage("Creator ID is required");
    }

    [Fact]
    public void Validate_WithValidMaxAttendees_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { MaxAttendees = 50 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxAttendees);
    }

    [Fact]
    public void Validate_WithMaxLengthTitle_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { Title = new string('a', 200) }; // Exactly 200 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithMaxLengthDescription_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { Description = new string('a', 2000) }; // Exactly 2000 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_WithMaxLengthLocation_ShouldNotHaveValidationError()
    {
        // Arrange
        var command = GetValidCommand();
        command = command with { Location = new string('a', 500) }; // Exactly 500 characters

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Location);
    }

    private static CreateMeetingCommand GetValidCommand()
    {
        return new CreateMeetingCommand
        {
            Title = "Valid Meeting Title",
            Description = "Valid meeting description",
            Location = "Conference Room A",
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxAttendees = 10,
            CreatorId = Guid.NewGuid()
        };
    }
}
