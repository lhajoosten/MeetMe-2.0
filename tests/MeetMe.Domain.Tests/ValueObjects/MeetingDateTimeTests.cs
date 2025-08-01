using FluentAssertions;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Domain.Tests.ValueObjects;

public class MeetingDateTimeTests
{
    [Fact]
    public void Create_WithValidFutureDates_ShouldCreateMeetingDateTime()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddHours(1);
        var endDateTime = DateTime.UtcNow.AddHours(2);

        // Act
        var meetingDateTime = MeetingDateTime.Create(startDateTime, endDateTime);

        // Assert
        meetingDateTime.Should().NotBeNull();
        meetingDateTime.StartDateTime.Should().Be(startDateTime);
        meetingDateTime.EndDateTime.Should().Be(endDateTime);
    }

    [Fact]
    public void Create_WithStartDateEqualToEndDate_ShouldThrowArgumentException()
    {
        // Arrange
        var dateTime = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        var action = () => MeetingDateTime.Create(dateTime, dateTime);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Start date must be before end date");
    }

    [Fact]
    public void Create_WithStartDateAfterEndDate_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddHours(2);
        var endDateTime = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        var action = () => MeetingDateTime.Create(startDateTime, endDateTime);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Start date must be before end date");
    }

    [Fact]
    public void Create_WithPastStartDate_ShouldThrowArgumentException()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddHours(-1);
        var endDateTime = DateTime.UtcNow.AddHours(1);

        // Act & Assert
        var action = () => MeetingDateTime.Create(startDateTime, endDateTime);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Meeting cannot be scheduled in the past");
    }

    [Fact]
    public void Create_WithCurrentTime_ShouldThrowArgumentException()
    {
        // Arrange
        var currentTime = DateTime.UtcNow;
        var endDateTime = currentTime.AddHours(1);

        // Act & Assert
        var action = () => MeetingDateTime.Create(currentTime, endDateTime);
        action.Should().Throw<ArgumentException>()
            .WithMessage("Meeting cannot be scheduled in the past");
    }

    [Fact]
    public void Duration_ShouldReturnCorrectTimeSpan()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddHours(1);
        var endDateTime = DateTime.UtcNow.AddHours(3);
        var meetingDateTime = MeetingDateTime.Create(startDateTime, endDateTime);

        // Act
        var duration = meetingDateTime.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromHours(2));
    }

    [Fact]
    public void IsToday_WhenStartDateIsToday_ShouldReturnTrue()
    {
        // Arrange
        var today = DateTime.UtcNow.Date.AddHours(DateTime.UtcNow.Hour + 1); // Today but in the future
        var endDateTime = today.AddHours(1);
        var meetingDateTime = MeetingDateTime.Create(today, endDateTime);

        // Act
        var isToday = meetingDateTime.IsToday;

        // Assert
        isToday.Should().BeTrue();
    }

    [Fact]
    public void IsToday_WhenStartDateIsNotToday_ShouldReturnFalse()
    {
        // Arrange
        var tomorrow = DateTime.UtcNow.AddDays(1);
        var endDateTime = tomorrow.AddHours(1);
        var meetingDateTime = MeetingDateTime.Create(tomorrow, endDateTime);

        // Act
        var isToday = meetingDateTime.IsToday;

        // Assert
        isToday.Should().BeFalse();
    }

    [Fact]
    public void IsUpcoming_WhenStartDateIsInFuture_ShouldReturnTrue()
    {
        // Arrange
        var futureDateTime = DateTime.UtcNow.AddHours(1);
        var endDateTime = futureDateTime.AddHours(1);
        var meetingDateTime = MeetingDateTime.Create(futureDateTime, endDateTime);

        // Act
        var isUpcoming = meetingDateTime.IsUpcoming;

        // Assert
        isUpcoming.Should().BeTrue();
    }

    [Theory]
    [InlineData(1, 2, 1)] // 1 hour duration
    [InlineData(1, 3, 2)] // 2 hour duration
    [InlineData(1, 1.5, 0.5)] // 30 minutes duration
    public void Duration_WithDifferentTimeSpans_ShouldReturnCorrectDuration(double startHoursFromNow, double endHoursFromNow, double expectedDurationHours)
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddHours(startHoursFromNow);
        var endDateTime = DateTime.UtcNow.AddHours(endHoursFromNow);
        var meetingDateTime = MeetingDateTime.Create(startDateTime, endDateTime);

        // Act
        var duration = meetingDateTime.Duration;

        // Assert
        duration.Should().Be(TimeSpan.FromHours(expectedDurationHours));
    }

    [Fact]
    public void EqualityComparison_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddHours(1);
        var endDateTime = DateTime.UtcNow.AddHours(2);
        var meetingDateTime1 = MeetingDateTime.Create(startDateTime, endDateTime);
        var meetingDateTime2 = MeetingDateTime.Create(startDateTime, endDateTime);

        // Act & Assert
        meetingDateTime1.Should().Be(meetingDateTime2);
        meetingDateTime1.GetHashCode().Should().Be(meetingDateTime2.GetHashCode());
    }

    [Fact]
    public void EqualityComparison_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var startDateTime1 = DateTime.UtcNow.AddHours(1);
        var endDateTime1 = DateTime.UtcNow.AddHours(2);
        var startDateTime2 = DateTime.UtcNow.AddHours(2);
        var endDateTime2 = DateTime.UtcNow.AddHours(3);
        var meetingDateTime1 = MeetingDateTime.Create(startDateTime1, endDateTime1);
        var meetingDateTime2 = MeetingDateTime.Create(startDateTime2, endDateTime2);

        // Act & Assert
        meetingDateTime1.Should().NotBe(meetingDateTime2);
    }

    [Fact]
    public void Create_WithMinimumValidTimeGap_ShouldCreateMeetingDateTime()
    {
        // Arrange
        var startDateTime = DateTime.UtcNow.AddMilliseconds(1);
        var endDateTime = startDateTime.AddMilliseconds(1);

        // Act
        var meetingDateTime = MeetingDateTime.Create(startDateTime, endDateTime);

        // Assert
        meetingDateTime.Should().NotBeNull();
        meetingDateTime.Duration.Should().Be(TimeSpan.FromMilliseconds(1));
    }
}
