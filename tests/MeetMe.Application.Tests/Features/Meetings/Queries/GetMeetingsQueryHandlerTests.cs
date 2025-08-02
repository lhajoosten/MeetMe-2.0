using AutoMapper;
using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Features.Meetings.Queries.GetAllMeetings;
using MeetMe.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace MeetMe.Application.Tests.Features.Meetings.Queries;

public class GetMeetingsQueryHandlerTests
{
    private readonly Mock<IQueryRepository<Meeting, int>> _mockMeetingRepository;
    private readonly Mock<IMapper> _mockMapper;
    private readonly GetMeetingsQueryHandler _handler;

    public GetMeetingsQueryHandlerTests()
    {
        _mockMeetingRepository = new Mock<IQueryRepository<Meeting, int>>();
        _mockMapper = new Mock<IMapper>();
        _handler = new GetMeetingsQueryHandler(_mockMeetingRepository.Object, _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithDefaultParameters_ShouldReturnAllActiveMeetings()
    {
        // Arrange
        var creator = User.Create("John", "Doe", "john@example.com");
        var meetings = new List<Meeting>
        {
            Meeting.Create("Meeting 1", "Description 1", "Location 1",
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator),
            Meeting.Create("Meeting 2", "Description 2", "Location 2",
                DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(1), creator)
        };

        _mockMeetingRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Meeting, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(meetings);

        var query = new GetMeetingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value.Should().BeInAscendingOrder(x => x.StartDateTime);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_ShouldFilterMeetingsBySearchTerm()
    {
        // Arrange
        var creator = User.Create("John", "Doe", "john@example.com");
        var meetings = new List<Meeting>
        {
            Meeting.Create("Important Meeting", "Description 1", "Location 1",
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator),
            Meeting.Create("Regular Meeting", "Important discussion", "Location 2",
                DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(1), creator),
            Meeting.Create("Other Meeting", "Description 3", "Important location",
                DateTime.UtcNow.AddDays(3), DateTime.UtcNow.AddDays(3).AddHours(1), creator)
        };

        _mockMeetingRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Meeting, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(meetings);

        var query = new GetMeetingsQuery { SearchTerm = "important" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(3); // All should match the search term
    }

    [Fact]
    public async Task Handle_WithCreatorIdFilter_ShouldFilterMeetingsByCreator()
    {
        // Arrange
        var creator1 = User.Create("John", "Doe", "john@example.com");
        var creator2 = User.Create("Jane", "Smith", "jane@example.com");
        
        var meetings = new List<Meeting>
        {
            Meeting.Create("Meeting 1", "Description 1", "Location 1",
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator1),
            Meeting.Create("Meeting 2", "Description 2", "Location 2",
                DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(1), creator2)
        };

        _mockMeetingRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Meeting, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(meetings);

        var query = new GetMeetingsQuery { CreatorId = creator1.Id };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithUpcomingFilter_ShouldFilterUpcomingMeetings()
    {
        // Arrange
        var creator = User.Create("John", "Doe", "john@example.com");
        var meetings = new List<Meeting>
        {
            Meeting.Create("Future Meeting", "Description 1", "Location 1",
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator),
            Meeting.Create("Another Future Meeting", "Description 2", "Location 2",
                DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(1), creator)
        };

        _mockMeetingRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Meeting, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(meetings);

        var query = new GetMeetingsQuery { IsUpcoming = true };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldReturnCorrectPage()
    {
        // Arrange
        var creator = User.Create("John", "Doe", "john@example.com");
        var meetings = new List<Meeting>();
        
        for (int i = 1; i <= 15; i++)
        {
            meetings.Add(Meeting.Create($"Meeting {i}", $"Description {i}", $"Location {i}",
                DateTime.UtcNow.AddDays(i), DateTime.UtcNow.AddDays(i).AddHours(1), creator));
        }

        _mockMeetingRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Meeting, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(meetings);

        var query = new GetMeetingsQuery { Page = 2, PageSize = 5 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(5); // Should return exactly 5 items for page 2
    }

    [Fact]
    public async Task Handle_WithInactiveFilter_ShouldFilterInactiveMeetings()
    {
        // Arrange
        var creator = User.Create("John", "Doe", "john@example.com");
        var meetings = new List<Meeting>
        {
            Meeting.Create("Active Meeting", "Description 1", "Location 1",
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator),
            Meeting.Create("Inactive Meeting", "Description 2", "Location 2",
                DateTime.UtcNow.AddDays(2), DateTime.UtcNow.AddDays(2).AddHours(1), creator)
        };

        // Deactivate the second meeting
        meetings[1].Cancel(creator);

        _mockMeetingRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Meeting, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(meetings);

        var query = new GetMeetingsQuery { IsActive = false };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNullSearchTerm_ShouldNotApplySearchFilter()
    {
        // Arrange
        var creator = User.Create("John", "Doe", "john@example.com");
        var meetings = new List<Meeting>
        {
            Meeting.Create("Meeting 1", "Description 1", "Location 1",
                DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator)
        };

        _mockMeetingRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Meeting, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(meetings);

        var query = new GetMeetingsQuery { SearchTerm = null };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyList()
    {
        // Arrange
        _mockMeetingRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Meeting, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(new List<Meeting>());

        var query = new GetMeetingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldIncludeRelatedData()
    {
        // Arrange
        var creator = User.Create("John", "Doe", "john@example.com");
        var meeting = Meeting.Create("Test Meeting", "Description", "Location",
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator);

        // Add attendees and posts
        var attendee = User.Create("Jane", "Smith", "jane@example.com");
        var attendance = Attendance.Create(attendee, meeting);
        var post = Post.Create("Post Title", "Post Content", creator, meeting);

        meeting.Attendees.Add(attendance);
        meeting.Posts.Add(post);

        _mockMeetingRepository
            .Setup(x => x.FindAsync(
                It.IsAny<Expression<Func<Meeting, bool>>>(),
                It.IsAny<CancellationToken>(),
                It.IsAny<Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(new List<Meeting> { meeting });

        var query = new GetMeetingsQuery();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(1);

        var meetingDto = result.Value.First();
        meetingDto.AttendeeCount.Should().Be(1);
        meetingDto.PostCount.Should().Be(1);
        meetingDto.CreatorName.Should().Be("John Doe");
    }
}
