using FluentAssertions;
using FluentValidation.TestHelper;
using MeetMe.Application.Features.Meetings.Commands.UpdateMeeting;

namespace MeetMe.Application.Tests.Features.Meetings.Commands.UpdateMeeting;

public class UpdateMeetingCommandValidatorTests
{
    private readonly UpdateMeetingCommandValidator _validator;

    public UpdateMeetingCommandValidatorTests()
    {
        _validator = new UpdateMeetingCommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Empty()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.Empty,
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Meeting ID is required");
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Should_Have_Error_When_Title_Exceeds_Maximum_Length()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = new string('a', 201), // 201 characters
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description is required");
    }

    [Fact]
    public void Should_Have_Error_When_Description_Exceeds_Maximum_Length()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = new string('a', 2001), // 2001 characters
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 2000 characters");
    }

    [Fact]
    public void Should_Have_Error_When_StartDateTime_Is_Empty()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = default,
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDateTime)
            .WithErrorMessage("Start date and time is required");
    }

    [Fact]
    public void Should_Have_Error_When_StartDateTime_Is_In_The_Past()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(-1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StartDateTime)
            .WithErrorMessage("Start date must be in the future");
    }

    [Fact]
    public void Should_Have_Error_When_EndDateTime_Is_Empty()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = default,
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDateTime)
            .WithErrorMessage("End date and time is required");
    }

    [Fact]
    public void Should_Have_Error_When_EndDateTime_Is_Before_StartDateTime()
    {
        // Arrange
        var startDate = DateTime.Now.AddDays(1);
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = startDate,
            EndDateTime = startDate.AddHours(-1), // End before start
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EndDateTime)
            .WithErrorMessage("End date must be after start date");
    }

    [Fact]
    public void Should_Have_Error_When_Location_Is_Empty()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location is required");
    }

    [Fact]
    public void Should_Have_Error_When_Location_Exceeds_Maximum_Length()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = new string('a', 501), // 501 characters
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location must not exceed 500 characters");
    }

    [Fact]
    public void Should_Have_Error_When_MaxAttendees_Is_Zero()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 0,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxAttendees)
            .WithErrorMessage("Maximum attendees must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_MaxAttendees_Is_Negative()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = -1,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxAttendees)
            .WithErrorMessage("Maximum attendees must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_MaxAttendees_Exceeds_Maximum()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 1001,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxAttendees)
            .WithErrorMessage("Maximum attendees cannot exceed 1000");
    }

    [Fact]
    public void Should_Have_Error_When_OrganizerId_Is_Empty()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.OrganizerId)
            .WithErrorMessage("Organizer ID is required");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(1000)]
    public void Should_Not_Have_Error_When_MaxAttendees_Is_Valid(int maxAttendees)
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = "Valid Title",
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = maxAttendees,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxAttendees);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(200)]
    public void Should_Not_Have_Error_When_Title_Length_Is_Valid(int titleLength)
    {
        // Arrange
        var command = new UpdateMeetingCommand
        {
            Id = Guid.NewGuid(),
            Title = new string('a', titleLength),
            Description = "Valid Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Valid Location",
            MaxAttendees = 10,
            OrganizerId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }
}
