using FluentAssertions;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Domain.Tests.ValueObjects;

public class LocationTests
{
    [Theory]
    [InlineData("Conference Room A")]
    [InlineData("123 Main Street, City")]
    [InlineData("Online Meeting")]
    [InlineData("Building 1, Floor 3, Room 301")]
    public void Create_WithValidLocation_ShouldCreateLocation(string validLocation)
    {
        // Act
        var location = Location.Create(validLocation);

        // Assert
        location.Should().NotBeNull();
        location.Value.Should().Be(validLocation.Trim());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpaceLocation_ShouldThrowArgumentException(string invalidLocation)
    {
        // Act & Assert
        var action = () => Location.Create(invalidLocation);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithLocationTooLong_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var longLocation = new string('a', 501); // Exceeds MaxLength of 500

        // Act & Assert
        var action = () => Location.Create(longLocation);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Arrange
        var locationWithWhitespace = "  Conference Room A  ";

        // Act
        var location = Location.Create(locationWithWhitespace);

        // Assert
        location.Value.Should().Be("Conference Room A");
    }

    [Fact]
    public void Create_WithMaxLength_ShouldCreateLocation()
    {
        // Arrange
        var maxLengthLocation = new string('a', 500); // Exactly MaxLength

        // Act
        var location = Location.Create(maxLengthLocation);

        // Assert
        location.Should().NotBeNull();
        location.Value.Should().Be(maxLengthLocation);
    }

    [Fact]
    public void GetEqualityComponents_ShouldReturnValue()
    {
        // Arrange
        var location1 = Location.Create("Conference Room A");
        var location2 = Location.Create("Conference Room A");
        var location3 = Location.Create("Conference Room B");

        // Act & Assert
        location1.Should().Be(location2);
        location1.Should().NotBe(location3);
        location1.GetHashCode().Should().Be(location2.GetHashCode());
        location1.GetHashCode().Should().NotBe(location3.GetHashCode());
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var location1 = Location.Create("Conference Room A");
        var location2 = Location.Create("Conference Room A");

        // Act & Assert
        location1.Should().Be(location2);
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var location1 = Location.Create("Conference Room A");
        var location2 = Location.Create("Conference Room B");

        // Act & Assert
        location1.Should().NotBe(location2);
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var locationValue = "Conference Room A";
        var location = Location.Create(locationValue);

        // Act
        var result = location.ToString();

        // Assert
        result.Should().Be(locationValue);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnValue()
    {
        // Arrange
        var locationValue = "Conference Room A";
        var location = Location.Create(locationValue);

        // Act
        string result = location; // Implicit conversion

        // Assert
        result.Should().Be(locationValue);
    }

    [Fact]
    public void Create_WithWhitespaceOnly_AfterTrim_ShouldThrowArgumentException()
    {
        // Arrange
        var whitespaceLocation = "   ";

        // Act & Assert
        var action = () => Location.Create(whitespaceLocation);
        action.Should().Throw<ArgumentException>();
    }
}
