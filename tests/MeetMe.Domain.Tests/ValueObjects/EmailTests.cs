using FluentAssertions;
using MeetMe.Domain.Exceptions;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@domain.org")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("123@456.789")]
    public void Create_WithValidEmail_ShouldCreateEmail(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Should().NotBeNull();
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithNullOrWhiteSpaceEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Act & Assert
        var action = () => Email.Create(invalidEmail);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user.example.com")]
    [InlineData("user@@example.com")]
    [InlineData("user@.com")]
    [InlineData("user@com")]
    public void Create_WithInvalidEmailFormat_ShouldThrowDomainException(string invalidEmail)
    {
        // Act & Assert
        var action = () => Email.Create(invalidEmail);
        action.Should().Throw<DomainException>()
            .WithMessage($"Invalid email format: {invalidEmail}");
    }

    [Fact]
    public void Create_WithEmailTooLong_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var longEmail = new string('a', 310) + "@example.com"; // Total length > 320

        // Act & Assert
        var action = () => Email.Create(longEmail);
        action.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Create_ShouldNormalizeToLowercase()
    {
        // Arrange
        var upperCaseEmail = "USER@EXAMPLE.COM";

        // Act
        var email = Email.Create(upperCaseEmail);

        // Assert
        email.Value.Should().Be("user@example.com");
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.org", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@example.com", false)]
    [InlineData("user@", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_ShouldReturnCorrectValidationResult(string email, bool expectedResult)
    {
        // Act
        var result = Email.IsValidEmail(email);

        // Assert
        result.Should().Be(expectedResult);
    }

    [Fact]
    public void IsValidEmail_WithEmailTooLong_ShouldReturnFalse()
    {
        // Arrange
        var longEmail = new string('a', 310) + "@example.com"; // Total length > 320

        // Act
        var result = Email.IsValidEmail(longEmail);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GetEqualityComponents_ShouldReturnValue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("test@example.com");
        var email3 = Email.Create("different@example.com");

        // Act & Assert
        email1.Should().Be(email2);
        email1.Should().NotBe(email3);
        email1.GetHashCode().Should().Be(email2.GetHashCode());
        email1.GetHashCode().Should().NotBe(email3.GetHashCode());
    }

    [Fact]
    public void Equals_WithSameValue_ShouldReturnTrue()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("TEST@EXAMPLE.COM"); // Different case

        // Act & Assert
        email1.Should().Be(email2); // Should be equal because both are normalized to lowercase
    }

    [Fact]
    public void Equals_WithDifferentValue_ShouldReturnFalse()
    {
        // Arrange
        var email1 = Email.Create("test@example.com");
        var email2 = Email.Create("different@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
    }
}
