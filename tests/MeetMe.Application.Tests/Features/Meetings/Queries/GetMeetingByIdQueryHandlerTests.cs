using AutoMapper;
using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Features.Meetings.Queries.GetMeeting;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;
using Moq;

namespace MeetMe.Application.Tests.Features.Meetings.Queries;

public class GetMeetingByIdQueryHandlerTests
{
    private readonly Mock<IQueryRepository<Meeting, int>> _mockMeetingRepository;
    private readonly GetMeetingByIdQueryHandler _handler;
    private readonly Mock<IMapper> _mapper;

    public GetMeetingByIdQueryHandlerTests()
    {
        _mockMeetingRepository = new Mock<IQueryRepository<Meeting, int>>();
        _mapper = new Mock<IMapper>();
        _handler = new GetMeetingByIdQueryHandler(_mockMeetingRepository.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldReturnMeetingDto()
    {
        // Arrange
        var meetingId= 1;
        var creator = User.Create("John", "Doe", "john@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Conference Room",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            creator,
            10
        );

        // Use reflection to set the Id since it's likely set by EF Core
        var meetingType = typeof(Meeting);
        var idProperty = meetingType.GetProperty("Id");
        idProperty?.SetValue(meeting, meetingId);

        _mockMeetingRepository
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(meeting);

        var query = new GetMeetingByIdQuery(meetingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(meetingId);
        result.Value.Title.Should().Be("Test Meeting");
        result.Value.Description.Should().Be("Test Description");
        result.Value.Location.Should().Be("Conference Room");
        result.Value.CreatorId.Should().Be(creator.Id);
        result.Value.Creator.FullName.Should().Be($"{creator.FirstName} {creator.LastName}");
        result.Value.IsUpcoming.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidId_ShouldIncludeRelatedData()
    {
        // Arrange
        var meetingId= 1;
        var creator = User.Create("John", "Doe", "john@example.com");
        var meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Conference Room",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            creator,
            10
        );

        // Simulate attendees and posts
        var attendee1 = User.Create("Jane", "Smith", "jane@example.com");
        var attendee2 = User.Create("Bob", "Johnson", "bob@example.com");
        var attendance1 = Attendance.Create(attendee1, meeting);
        var attendance2 = Attendance.Create(attendee2, meeting);

        var post1 = Post.Create("Post 1", "Content 1", creator, meeting);
        var post2 = Post.Create("Post 2", "Content 2", attendee1, meeting);

        // Add to collections
        meeting.Attendees.Add(attendance1);
        meeting.Attendees.Add(attendance2);
        meeting.Posts.Add(post1);
        meeting.Posts.Add(post2);

        _mockMeetingRepository
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Meeting, object>>[]>()))
            .ReturnsAsync(meeting);

        var query = new GetMeetingByIdQuery(meetingId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Attendees.Count.Should().Be(2);
        result.Value.Posts.Count.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithCancelledToken_ShouldPassCancellationToken()
    {
        // Arrange
        var meetingId= 1;
        var cancellationToken = new CancellationToken(true);
        var query = new GetMeetingByIdQuery(meetingId);

        _mockMeetingRepository
            .Setup(x => x.GetByIdAsync(
                meetingId,
                cancellationToken,
                It.IsAny<System.Linq.Expressions.Expression<Func<Meeting, object>>[]>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            _handler.Handle(query, cancellationToken));

        _mockMeetingRepository.Verify(x => x.GetByIdAsync(
            meetingId,
            cancellationToken,
            It.IsAny<System.Linq.Expressions.Expression<Func<Meeting, object>>[]>()), Times.Once);
    }

    [Fact]
    public void Query_Constructor_ShouldSetIdCorrectly()
    {
        // Arrange
        var id = 1;

        // Act
        var query = new GetMeetingByIdQuery(id);

        // Assert
        query.Id.Should().Be(id);
    }

    [Fact]
    public async Task Handle_ShouldCallRepositoryWithCorrectIncludes()
    {
        // Arrange
        var meetingId= 1;
        var query = new GetMeetingByIdQuery(meetingId);

        _mockMeetingRepository
            .Setup(x => x.GetByIdAsync(
                meetingId,
                It.IsAny<CancellationToken>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Meeting, object>>[]>()))!
            .ReturnsAsync((Meeting?)null);

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _mockMeetingRepository.Verify(x => x.GetByIdAsync(
            meetingId,
            It.IsAny<CancellationToken>(),
            It.Is<System.Linq.Expressions.Expression<Func<Meeting, object>>[]>(
                includes => includes.Length == 3)), Times.Once);
    }
}
