using FluentAssertions;
using MeetMe.Domain.Entities;
using MeetMe.Domain.Events;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Domain.Tests.Entities;

public class MeetingTests
{
    private User GetTestUser()
    {
        return User.Create("John", "Doe", "john@example.com", "Developer");
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateMeeting()
    {
        // Arrange
        var title = "Test Meeting";
        var description = "Test Description";
        var location = "Conference Room A";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var creator = GetTestUser();

        // Act
        var meeting = Meeting.Create(title, description, location, startDateTime, endDateTime, creator);

        // Assert
        meeting.Should().NotBeNull();
        meeting.Title.Should().Be(title);
        meeting.Description.Should().Be(description);
        meeting.Location.Value.Should().Be(location);
        meeting.MeetingDateTime.StartDateTime.Should().Be(startDateTime);
        meeting.MeetingDateTime.EndDateTime.Should().Be(endDateTime);
        meeting.Creator.Should().Be(creator);
        meeting.CreatorId.Should().Be(creator.Id);
        meeting.MaxAttendees.Should().BeNull();
        meeting.IsActive.Should().BeTrue();
        meeting.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Create_WithMaxAttendees_ShouldSetMaxAttendees()
    {
        // Arrange
        var title = "Test Meeting";
        var description = "Test Description";
        var location = "Conference Room A";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var creator = GetTestUser();
        var maxAttendees = 10;

        // Act
        var meeting = Meeting.Create(title, description, location, startDateTime, endDateTime, creator, maxAttendees);

        // Assert
        meeting.MaxAttendees.Should().Be(maxAttendees);
    }

    [Fact]
    public void Create_ShouldAddMeetingCreatedEvent()
    {
        // Arrange
        var title = "Test Meeting";
        var description = "Test Description";
        var location = "Conference Room A";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var creator = GetTestUser();

        // Act
        var meeting = Meeting.Create(title, description, location, startDateTime, endDateTime, creator);

        // Assert
        meeting.DomainEvents.Should().HaveCount(1);
        meeting.DomainEvents.First().Should().BeOfType<MeetingCreatedEvent>();
        var meetingCreatedEvent = (MeetingCreatedEvent)meeting.DomainEvents.First();
        meetingCreatedEvent.Meeting.Should().Be(meeting);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Arrange
        var description = "Test Description";
        var location = "Conference Room A";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var creator = GetTestUser();

        // Act & Assert
        var action = () => Meeting.Create(invalidTitle, description, location, startDateTime, endDateTime, creator);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidDescription_ShouldThrowArgumentException(string invalidDescription)
    {
        // Arrange
        var title = "Test Meeting";
        var location = "Conference Room A";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);
        var creator = GetTestUser();

        // Act & Assert
        var action = () => Meeting.Create(title, invalidDescription, location, startDateTime, endDateTime, creator);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullCreator_ShouldThrowArgumentNullException()
    {
        // Arrange
        var title = "Test Meeting";
        var description = "Test Description";
        var location = "Conference Room A";
        var startDateTime = DateTime.UtcNow.AddDays(1);
        var endDateTime = startDateTime.AddHours(1);

        // Act & Assert
        var action = () => Meeting.Create(title, description, location, startDateTime, endDateTime, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void UpdateDetails_WithValidData_ShouldUpdateMeeting()
    {
        // Arrange
        var creator = GetTestUser();
        var meeting = Meeting.Create("Original Title", "Original Description", "Original Location", 
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator);

        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newLocation = "Updated Location";
        var newStartDateTime = DateTime.UtcNow.AddDays(2);
        var newEndDateTime = newStartDateTime.AddHours(2);
        var updatingUser = GetTestUser();

        // Act
        meeting.UpdateDetails(newTitle, newDescription, newLocation, newStartDateTime, newEndDateTime, updatingUser);

        // Assert
        meeting.Title.Should().Be(newTitle);
        meeting.Description.Should().Be(newDescription);
        meeting.Location.Value.Should().Be(newLocation);
        meeting.MeetingDateTime.StartDateTime.Should().Be(newStartDateTime);
        meeting.MeetingDateTime.EndDateTime.Should().Be(newEndDateTime);
        meeting.LastModifiedByUserId.Should().Be(updatingUser.Id.ToString());
        meeting.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CanAcceptMoreAttendees_WithNoMaxAttendees_ShouldReturnTrue()
    {
        // Arrange
        var creator = GetTestUser();
        var meeting = Meeting.Create("Test Meeting", "Test Description", "Conference Room A", 
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator);

        // Act
        var result = meeting.CanAcceptMoreAttendees();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAcceptMoreAttendees_WithMaxAttendeesNotReached_ShouldReturnTrue()
    {
        // Arrange
        var creator = GetTestUser();
        var meeting = Meeting.Create("Test Meeting", "Test Description", "Conference Room A", 
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator, maxAttendees: 5);

        // Act
        var result = meeting.CanAcceptMoreAttendees();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUpcoming_WithFutureStartTime_ShouldReturnTrue()
    {
        // Arrange
        var creator = GetTestUser();
        var futureDateTime = DateTime.UtcNow.AddDays(1);
        var meeting = Meeting.Create("Test Meeting", "Test Description", "Conference Room A", 
            futureDateTime, futureDateTime.AddHours(1), creator);

        // Act
        var result = meeting.IsUpcoming();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsUpcoming_WithPastStartTime_ShouldNotBeAllowedToCreate()
    {
        // Arrange
        var creator = GetTestUser();
        var pastDateTime = DateTime.UtcNow.AddDays(-1);

        // Act & Assert - Should not allow creating meetings in the past
        var action = () => Meeting.Create("Test Meeting", "Test Description", "Conference Room A", 
            pastDateTime, pastDateTime.AddHours(1), creator);

        action.Should().Throw<ArgumentException>()
            .WithMessage("Meeting cannot be scheduled in the past");
    }

    [Fact]
    public void Cancel_ShouldSetIsActiveToFalseAndUpdateModificationInfo()
    {
        // Arrange
        var creator = GetTestUser();
        var meeting = Meeting.Create("Test Meeting", "Test Description", "Conference Room A", 
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator);
        var cancellingUser = GetTestUser();

        // Act
        meeting.Cancel(cancellingUser);

        // Assert
        meeting.IsActive.Should().BeFalse();
        meeting.LastModifiedByUserId.Should().Be(cancellingUser.Id.ToString());
        meeting.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AddDomainEvent_ShouldAddEventToDomainEvents()
    {
        // Arrange
        var creator = GetTestUser();
        var meeting = Meeting.Create("Test Meeting", "Test Description", "Conference Room A", 
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator);
        var customEvent = new MeetingCreatedEvent(meeting);

        // Act
        meeting.AddDomainEvent(customEvent);

        // Assert
        meeting.DomainEvents.Should().Contain(customEvent);
        meeting.DomainEvents.Should().HaveCount(2); // Original create event + custom event
    }

    [Fact]
    public void RemoveDomainEvent_ShouldRemoveEventFromDomainEvents()
    {
        // Arrange
        var creator = GetTestUser();
        var meeting = Meeting.Create("Test Meeting", "Test Description", "Conference Room A", 
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator);
        var originalEvent = meeting.DomainEvents.First();

        // Act
        meeting.RemoveDomainEvent(originalEvent);

        // Assert
        meeting.DomainEvents.Should().NotContain(originalEvent);
        meeting.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var creator = GetTestUser();
        var meeting = Meeting.Create("Test Meeting", "Test Description", "Conference Room A", 
            DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(1).AddHours(1), creator);
        meeting.AddDomainEvent(new MeetingCreatedEvent(meeting));

        // Act
        meeting.ClearDomainEvents();

        // Assert
        meeting.DomainEvents.Should().BeEmpty();
    }
}
