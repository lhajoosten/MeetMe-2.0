using FluentAssertions;
using MeetMe.Domain.Entities;

namespace MeetMe.Domain.Tests.Entities;

public class AttendanceStatusTests
{
    [Fact]
    public void Confirmed_ShouldHaveCorrectIdAndName()
    {
        // Act & Assert
        AttendanceStatus.Confirmed.Id.Should().Be(1);
        AttendanceStatus.Confirmed.Name.Should().Be("Confirmed");
    }

    [Fact]
    public void Maybe_ShouldHaveCorrectIdAndName()
    {
        // Act & Assert
        AttendanceStatus.Maybe.Id.Should().Be(2);
        AttendanceStatus.Maybe.Name.Should().Be("Maybe");
    }

    [Fact]
    public void NotAttending_ShouldHaveCorrectIdAndName()
    {
        // Act & Assert
        AttendanceStatus.NotAttending.Id.Should().Be(3);
        AttendanceStatus.NotAttending.Name.Should().Be("Not Attending");
    }

    [Fact]
    public void List_ShouldReturnAllAttendanceStatuses()
    {
        // Act
        var statuses = AttendanceStatus.List().ToList();

        // Assert
        statuses.Should().HaveCount(3);
        statuses.Should().Contain(AttendanceStatus.Confirmed);
        statuses.Should().Contain(AttendanceStatus.Maybe);
        statuses.Should().Contain(AttendanceStatus.NotAttending);
    }

    [Fact]
    public void Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var status1 = AttendanceStatus.Confirmed;
        var status2 = AttendanceStatus.Confirmed;
        var status3 = AttendanceStatus.Maybe;

        // Act & Assert
        status1.Should().Be(status2);
        status1.Should().NotBe(status3);
        status1.GetHashCode().Should().Be(status2.GetHashCode());
        status1.GetHashCode().Should().NotBe(status3.GetHashCode());
    }

    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateAttendanceStatus()
    {
        // Arrange
        var id = 99;
        var name = "Custom Status";

        // Act
        var status = new AttendanceStatus(id, name);

        // Assert
        status.Id.Should().Be(id);
        status.Name.Should().Be(name);
    }

    [Fact]
    public void ToString_ShouldReturnName()
    {
        // Act & Assert
        AttendanceStatus.Confirmed.ToString().Should().Be("Confirmed");
        AttendanceStatus.Maybe.ToString().Should().Be("Maybe");
        AttendanceStatus.NotAttending.ToString().Should().Be("Not Attending");
    }

    [Theory]
    [InlineData(1, "Confirmed")]
    [InlineData(2, "Maybe")]
    [InlineData(3, "Not Attending")]
    public void StaticInstances_ShouldHaveCorrectValues(int expectedId, string expectedName)
    {
        // Arrange
        var allStatuses = AttendanceStatus.List();

        // Act
        var status = allStatuses.First(s => s.Id == expectedId);

        // Assert
        status.Id.Should().Be(expectedId);
        status.Name.Should().Be(expectedName);
    }

    [Fact]
    public void UniqueIds_ShouldBeUnique()
    {
        // Arrange
        var statuses = AttendanceStatus.List().ToList();

        // Act
        var uniqueIds = statuses.Select(s => s.Id).Distinct().ToList();

        // Assert
        uniqueIds.Should().HaveCount(statuses.Count);
    }

    [Fact]
    public void UniqueNames_ShouldBeUnique()
    {
        // Arrange
        var statuses = AttendanceStatus.List().ToList();

        // Act
        var uniqueNames = statuses.Select(s => s.Name).Distinct().ToList();

        // Assert
        uniqueNames.Should().HaveCount(statuses.Count);
    }
}
