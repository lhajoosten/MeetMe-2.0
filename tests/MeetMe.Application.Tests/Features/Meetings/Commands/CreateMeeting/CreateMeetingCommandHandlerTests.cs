using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Features.Meetings.Commands.CreateMeeting;
using MeetMe.Domain.Entities;
using Moq;

namespace MeetMe.Application.Tests.Features.Meetings.Commands.CreateMeeting;

public class CreateMeetingCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IQueryRepository<User, int>> _mockUserQueryRepository;
    private readonly Mock<ICommandRepository<Meeting, int>> _mockMeetingCommandRepository;
    private readonly CreateMeetingCommandHandler _handler;
    private readonly User _testUser;

    public CreateMeetingCommandHandlerTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockUserQueryRepository = new Mock<IQueryRepository<User, int>>();
        _mockMeetingCommandRepository = new Mock<ICommandRepository<Meeting, int>>();
        
        _mockUnitOfWork.Setup(x => x.CommandRepository<Meeting, int>())
            .Returns(_mockMeetingCommandRepository.Object);
        
        _handler = new CreateMeetingCommandHandler(_mockUnitOfWork.Object, _mockUserQueryRepository.Object);
        
        _testUser = User.Create("John", "Doe", "john@example.com");
        // Set the ID manually for testing purposes
        _testUser.GetType().GetProperty("Id")!.SetValue(_testUser, 1);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateMeetingAndReturnSuccess()
    {
        // Arrange
        var command = new CreateMeetingCommand
        {
            Title = "Test Meeting",
            Description = "Test Description",
            Location = "Conference Room A",
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxAttendees = 10,
            CreatorId = _testUser.Id
        };

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(0);

        _mockMeetingCommandRepository.Verify(
            x => x.AddAsync(It.Is<Meeting>(m => 
                m.Title == command.Title &&
                m.Description == command.Description &&
                m.Creator == _testUser &&
                m.MaxAttendees == command.MaxAttendees), 
                _testUser.Id, 
                It.IsAny<CancellationToken>()), 
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(_testUser.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentCreator_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateMeetingCommand
        {
            Title = "Test Meeting",
            Description = "Test Description",
            Location = "Conference Room A",
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxAttendees = 10,
            CreatorId = 1
        };

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(command.CreatorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Creator not found");

        _mockMeetingCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<Meeting>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithValidCommandAndNullMaxAttendees_ShouldCreateMeeting()
    {
        // Arrange
        var command = new CreateMeetingCommand
        {
            Title = "Test Meeting",
            Description = "Test Description",
            Location = "Conference Room A",
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxAttendees = null, // No limit
            CreatorId = _testUser.Id
        };

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(0);

        _mockMeetingCommandRepository.Verify(
            x => x.AddAsync(It.Is<Meeting>(m => 
                m.MaxAttendees == null), 
                _testUser.Id, 
                It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallUnitOfWorkSaveChangesWithCorrectUserId()
    {
        // Arrange
        var command = new CreateMeetingCommand
        {
            Title = "Test Meeting",
            Description = "Test Description",
            Location = "Conference Room A",
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxAttendees = 10,
            CreatorId = _testUser.Id
        };

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(_testUser.Id, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallAddAsyncWithCorrectParameters()
    {
        // Arrange
        var command = new CreateMeetingCommand
        {
            Title = "Important Meeting",
            Description = "Very important meeting description",
            Location = "Boardroom",
            StartDateTime = DateTime.UtcNow.AddDays(2),
            EndDateTime = DateTime.UtcNow.AddDays(2).AddHours(3),
            MaxAttendees = 25,
            CreatorId = _testUser.Id
        };

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockMeetingCommandRepository.Verify(
            x => x.AddAsync(
                It.Is<Meeting>(m => 
                    m.Title == "Important Meeting" &&
                    m.Description == "Very important meeting description" &&
                    m.MaxAttendees == 25 &&
                    m.Creator == _testUser),
                _testUser.Id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnMeetingIdFromCreatedMeeting()
    {
        // Arrange
        var command = new CreateMeetingCommand
        {
            Title = "Test Meeting",
            Description = "Test Description",
            Location = "Conference Room A",
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxAttendees = 10,
            CreatorId = _testUser.Id
        };

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(_testUser.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_testUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(0);
        
        // Verify that the returned ID is a valid integer (not default)
        result.Value.Should().NotBe(default(int));
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassCancellationTokenToRepositories()
    {
        // Arrange
        var command = new CreateMeetingCommand
        {
            Title = "Test Meeting",
            Description = "Test Description",
            Location = "Conference Room A",
            StartDateTime = DateTime.UtcNow.AddDays(1),
            EndDateTime = DateTime.UtcNow.AddDays(1).AddHours(2),
            MaxAttendees = 10,
            CreatorId = _testUser.Id
        };

        var cancellationToken = new CancellationToken();

        _mockUserQueryRepository
            .Setup(x => x.GetByIdAsync(_testUser.Id, cancellationToken))
            .ReturnsAsync(_testUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(1);

        // Act
        await _handler.Handle(command, cancellationToken);

        // Assert
        _mockUserQueryRepository.Verify(
            x => x.GetByIdAsync(_testUser.Id, cancellationToken),
            Times.Once);

        _mockMeetingCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<Meeting>(), It.IsAny<int>(), cancellationToken),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<int>(), cancellationToken),
            Times.Once);
    }
}
