using FluentAssertions;
using MediatR;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Meetings.Commands.UpdateMeeting;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;
using Moq;

namespace MeetMe.Application.Tests.Features.Meetings.Commands.UpdateMeeting;

public class UpdateMeetingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IQueryRepository<Meeting, int>> _mockMeetingQueryRepository;
    private readonly Mock<IQueryRepository<User, int>> _mockUserQueryRepository;
    private readonly Mock<ICommandRepository<Meeting, int>> _mockMeetingCommandRepository;
    private readonly UpdateMeetingCommandHandler _handler;

    public UpdateMeetingCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockMeetingQueryRepository = new Mock<IQueryRepository<Meeting, int>>();
        _mockUserQueryRepository = new Mock<IQueryRepository<User, int>>();
        _mockMeetingCommandRepository = new Mock<ICommandRepository<Meeting, int>>();

        _mockUnitOfWork
            .Setup(x => x.CommandRepository<Meeting, int>())
            .Returns(_mockMeetingCommandRepository.Object);

        _handler = new UpdateMeetingCommandHandler(
            _mockUnitOfWork.Object,
            _mockMeetingQueryRepository.Object,
            _mockUserQueryRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var meetingId = 1;
        var organizerId = 1;
        var command = new UpdateMeetingCommand
        {
            Id = meetingId,
            Title = "Updated Meeting Title",
            Description = "Updated Meeting Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Updated Location",
            MaxAttendees = 50,
            OrganizerId = organizerId
        };

        var originalOrganizer = User.Create("Jane", "Smith", Email.Create("jane@example.com"), "password", "Bio");
        var newOrganizer = User.Create("John", "Doe", Email.Create("john@example.com"), "password", "Bio");

        var existingMeeting = Meeting.Create(
            "Original Title",
            "Original Description",
            "Original Location",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(2).AddHours(1),
            originalOrganizer);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMeeting);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(organizerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOrganizer);

        _mockMeetingCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Meeting>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        _mockMeetingCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Meeting>(), organizerId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(organizerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentMeeting_ShouldReturnFailureResult()
    {
        // Arrange
        var meetingId = 1;
        var organizerId = 1;
        var command = new UpdateMeetingCommand
        {
            Id = meetingId,
            Title = "Updated Meeting Title",
            Description = "Updated Meeting Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Updated Location",
            MaxAttendees = 50,
            OrganizerId = organizerId
        };

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Meeting?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Meeting not found");

        _mockUserQueryRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockMeetingCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Meeting>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentOrganizer_ShouldReturnFailureResult()
    {
        // Arrange
        var meetingId = 1;
        var organizerId = 1;
        var command = new UpdateMeetingCommand
        {
            Id = meetingId,
            Title = "Updated Meeting Title",
            Description = "Updated Meeting Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Updated Location",
            MaxAttendees = 50,
            OrganizerId = organizerId
        };

        var originalOrganizer = User.Create("Jane", "Smith", Email.Create("jane@example.com"), "password", "Bio");

        var existingMeeting = Meeting.Create(
            "Original Title",
            "Original Description",
            "Original Location",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(2).AddHours(1),
            originalOrganizer);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMeeting);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(organizerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Organizer not found");

        _mockMeetingCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Meeting>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenMeetingQueryRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var meetingId = 1;
        var organizerId = 1;
        var command = new UpdateMeetingCommand
        {
            Id = meetingId,
            Title = "Updated Meeting Title",
            Description = "Updated Meeting Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Updated Location",
            MaxAttendees = 50,
            OrganizerId = organizerId
        };

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update meeting");
        result.Error.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task Handle_WhenUserQueryRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var meetingId = 1;
        var organizerId = 1;
        var command = new UpdateMeetingCommand
        {
            Id = meetingId,
            Title = "Updated Meeting Title",
            Description = "Updated Meeting Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Updated Location",
            MaxAttendees = 50,
            OrganizerId = organizerId
        };

        var originalOrganizer = User.Create("Jane", "Smith", Email.Create("jane@example.com"), "password", "Bio");

        var existingMeeting = Meeting.Create(
            "Original Title",
            "Original Description",
            "Original Location",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(2).AddHours(1),
            originalOrganizer);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMeeting);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(organizerId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("User service unavailable"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update meeting");
        result.Error.Should().Contain("User service unavailable");
    }

    [Fact]
    public async Task Handle_WhenUpdateAsyncThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var meetingId = 1;
        var organizerId = 1;
        var command = new UpdateMeetingCommand
        {
            Id = meetingId,
            Title = "Updated Meeting Title",
            Description = "Updated Meeting Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Updated Location",
            MaxAttendees = 50,
            OrganizerId = organizerId
        };

        var originalOrganizer = User.Create("Jane", "Smith", Email.Create("jane@example.com"), "password", "Bio");
        var newOrganizer = User.Create("John", "Doe", Email.Create("john@example.com"), "password", "Bio");

        var existingMeeting = Meeting.Create(
            "Original Title",
            "Original Description",
            "Original Location",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(2).AddHours(1),
            originalOrganizer);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMeeting);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(organizerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOrganizer);

        _mockMeetingCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Meeting>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to update meeting in database"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update meeting");
        result.Error.Should().Contain("Failed to update meeting in database");
    }

    [Fact]
    public async Task Handle_WhenSaveChangesAsyncThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var meetingId = 1;
        var organizerId = 1;
        var command = new UpdateMeetingCommand
        {
            Id = meetingId,
            Title = "Updated Meeting Title",
            Description = "Updated Meeting Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Updated Location",
            MaxAttendees = 50,
            OrganizerId = organizerId
        };

        var originalOrganizer = User.Create("Jane", "Smith", Email.Create("jane@example.com"), "password", "Bio");
        var newOrganizer = User.Create("John", "Doe", Email.Create("john@example.com"), "password", "Bio");

        var existingMeeting = Meeting.Create(
            "Original Title",
            "Original Description",
            "Original Location",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(2).AddHours(1),
            originalOrganizer);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMeeting);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(organizerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOrganizer);

        _mockMeetingCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Meeting>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save changes failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to update meeting");
        result.Error.Should().Contain("Save changes failed");
    }

    [Fact]
    public async Task Handle_ShouldCallMethodsWithCorrectParameters()
    {
        // Arrange
        var meetingId = 1;
        var organizerId = 1;
        var command = new UpdateMeetingCommand
        {
            Id = meetingId,
            Title = "Updated Meeting Title",
            Description = "Updated Meeting Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Updated Location",
            MaxAttendees = 50,
            OrganizerId = organizerId
        };

        var originalOrganizer = User.Create("Jane", "Smith", Email.Create("jane@example.com"), "password", "Bio");
        var newOrganizer = User.Create("John", "Doe", Email.Create("john@example.com"), "password", "Bio");

        var existingMeeting = Meeting.Create(
            "Original Title",
            "Original Description",
            "Original Location",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(2).AddHours(1),
            originalOrganizer);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingMeeting);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(organizerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newOrganizer);

        _mockMeetingCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Meeting>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockMeetingQueryRepository.Verify(
            x => x.GetByIdAsync(meetingId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUserQueryRepository.Verify(
            x => x.GetByIdAsync(organizerId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockMeetingCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Meeting>(), organizerId, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(organizerId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleCancellationToken()
    {
        // Arrange
        var meetingId = 1;
        var organizerId = 1;
        var cancellationToken = new CancellationToken(true);
        var command = new UpdateMeetingCommand
        {
            Id = meetingId,
            Title = "Updated Meeting Title",
            Description = "Updated Meeting Description",
            StartDateTime = DateTime.Now.AddDays(1),
            EndDateTime = DateTime.Now.AddDays(1).AddHours(2),
            Location = "Updated Location",
            MaxAttendees = 50,
            OrganizerId = organizerId
        };

        var originalOrganizer = User.Create("Jane", "Smith", Email.Create("jane@example.com"), "password", "Bio");
        var newOrganizer = User.Create("John", "Doe", Email.Create("john@example.com"), "password", "Bio");

        var existingMeeting = Meeting.Create(
            "Original Title",
            "Original Description",
            "Original Location",
            DateTime.Now.AddDays(2),
            DateTime.Now.AddDays(2).AddHours(1),
            originalOrganizer);

        _mockMeetingQueryRepository
            .Setup(x => x.GetByIdAsync(meetingId, cancellationToken))
            .ReturnsAsync(existingMeeting);

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(organizerId, cancellationToken))
            .ReturnsAsync(newOrganizer);

        _mockMeetingCommandRepository
            .Setup(x => x.UpdateAsync(It.IsAny<Meeting>(), It.IsAny<int>(), cancellationToken))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockMeetingQueryRepository.Verify(
            x => x.GetByIdAsync(meetingId, cancellationToken),
            Times.Once);

        _mockUserQueryRepository.Verify(
            x => x.GetByIdAsync(organizerId, cancellationToken),
            Times.Once);

        _mockMeetingCommandRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Meeting>(), It.IsAny<int>(), cancellationToken),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<int>(), cancellationToken),
            Times.Once);
    }
}
