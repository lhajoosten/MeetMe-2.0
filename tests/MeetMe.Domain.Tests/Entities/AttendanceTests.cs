using FluentAssertions;
using MeetMe.Domain.Common;
using MeetMe.Domain.Entities;
using MeetMe.Domain.Events;

namespace MeetMe.Domain.Tests.Entities;

public class AttendanceTests
{
    private readonly User _user;
    private readonly User _otherUser;
    private readonly Meeting _meeting;

    public AttendanceTests()
    {
        _user = User.Create("John", "Doe", "john@example.com");
        _otherUser = User.Create("Jane", "Smith", "jane@example.com");
        _meeting = Meeting.Create(
            "Test Meeting",
            "Test Description",
            "Conference Room",
            DateTime.UtcNow.AddDays(1),
            DateTime.UtcNow.AddDays(1).AddHours(1),
            _user,
            10
        );
    }

    [Fact]
    public void Create_WithValidParameters_ShouldCreateAttendance()
    {
        // Act
        var attendance = Attendance.Create(_user, _meeting);

        // Assert
        attendance.Should().NotBeNull();
        attendance.User.Should().Be(_user);
        attendance.Meeting.Should().Be(_meeting);
        attendance.Status.Should().Be(AttendanceStatus.Confirmed);
        attendance.JoinedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        attendance.IsActive.Should().BeTrue();
        attendance.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<UserJoinedMeetingEvent>();
    }

    [Fact]
    public void Create_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => Attendance.Create(null!, _meeting);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("User cannot be null when creating attendance.*");
    }

    [Fact]
    public void Create_WithNullMeeting_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => Attendance.Create(_user, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Meeting cannot be null when creating attendance.*");
    }

    [Fact]
    public void ChangeStatus_WithValidParameters_ShouldUpdateStatus()
    {
        // Arrange
        var attendance = Attendance.Create(_user, _meeting);
        var newStatus = AttendanceStatus.Maybe;
        var updatingUser = User.Create("Admin", "User", "admin@example.com");

        // Act
        attendance.ChangeStatus(newStatus, updatingUser);

        // Assert
        attendance.Status.Should().Be(newStatus);
        attendance.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        attendance.LastModifiedByUserId.Should().Be(updatingUser.Id.ToString());
    }

    [Theory]
    [MemberData(nameof(GetAllAttendanceStatuses))]
    public void ChangeStatus_WithDifferentStatuses_ShouldUpdateCorrectly(AttendanceStatus status)
    {
        // Arrange
        var attendance = Attendance.Create(_user, _meeting);
        var updatingUser = User.Create("Admin", "User", "admin@example.com");

        // Act
        attendance.ChangeStatus(status, updatingUser);

        // Assert
        attendance.Status.Should().Be(status);
        attendance.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        attendance.LastModifiedByUserId.Should().Be(updatingUser.Id.ToString());
    }

    public static IEnumerable<object[]> GetAllAttendanceStatuses()
    {
        yield return new object[] { AttendanceStatus.Confirmed };
        yield return new object[] { AttendanceStatus.Maybe };
        yield return new object[] { AttendanceStatus.NotAttending };
    }

    [Fact]
    public void ChangeStatus_WithNullStatus_ShouldThrowArgumentNullException()
    {
        // Arrange
        var attendance = Attendance.Create(_user, _meeting);
        var updatingUser = User.Create("Admin", "User", "admin@example.com");

        // Act & Assert
        var action = () => attendance.ChangeStatus(null!, updatingUser);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("Status cannot be null when changing attendance status.*");
    }

    [Fact]
    public void ChangeStatus_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var attendance = Attendance.Create(_user, _meeting);
        var newStatus = AttendanceStatus.Maybe;

        // Act & Assert
        var action = () => attendance.ChangeStatus(newStatus, null!);
        action.Should().Throw<ArgumentNullException>()
            .WithMessage("User cannot be null when changing attendance status.*");
    }

    [Fact]
    public void Leave_WithValidUser_ShouldDeactivateAttendanceAndRaiseDomainEvent()
    {
        // Arrange
        var attendance = Attendance.Create(_user, _meeting);
        var leavingUser = User.Create("Admin", "User", "admin@example.com");
        var initialEventCount = attendance.DomainEvents.Count;

        // Act
        attendance.Leave(leavingUser);

        // Assert
        attendance.IsActive.Should().BeFalse();
        attendance.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        attendance.LastModifiedByUserId.Should().Be(leavingUser.Id.ToString());
        attendance.DomainEvents.Should().HaveCount(initialEventCount + 1);
        attendance.DomainEvents.Should().Contain(e => e is UserLeftMeetingEvent);
    }

    [Fact]
    public void Leave_WithNullUser_ShouldThrowArgumentNullException()
    {
        // Arrange
        var attendance = Attendance.Create(_user, _meeting);

        // Act & Assert
        var action = () => attendance.Leave(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DomainEvents_Management_ShouldWorkCorrectly()
    {
        // Arrange
        var attendance = Attendance.Create(_user, _meeting);
        var initialEvent = attendance.DomainEvents.First();

        // Act & Assert - Initial state
        attendance.DomainEvents.Should().ContainSingle();

        // Act - Remove domain event
        attendance.RemoveDomainEvent(initialEvent);

        // Assert
        attendance.DomainEvents.Should().BeEmpty();

        // Act - Add domain event back
        attendance.AddDomainEvent(initialEvent);

        // Assert
        attendance.DomainEvents.Should().ContainSingle();

        // Act - Clear all domain events
        attendance.ClearDomainEvents();

        // Assert
        attendance.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void DefaultValues_ShouldBeSetCorrectly()
    {
        // Act
        var attendance = Attendance.Create(_user, _meeting);

        // Assert
        attendance.Status.Should().Be(AttendanceStatus.Confirmed);
        attendance.IsActive.Should().BeTrue();
        attendance.JoinedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }
}
