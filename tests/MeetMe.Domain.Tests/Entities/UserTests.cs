using FluentAssertions;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;

namespace MeetMe.Domain.Tests.Entities;

public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john@example.com";
        var bio = "Software Developer";

        // Act
        var user = User.Create(firstName, lastName, email, bio);

        // Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Value.Should().Be(email.ToLowerInvariant());
        user.Bio.Should().Be(bio);
        user.Id.Should().NotBe(0);
        user.IsActive.Should().BeTrue();
        user.FullName.Should().Be($"{firstName} {lastName}");
    }

    [Fact]
    public void Create_WithoutBio_ShouldCreateUserWithNullBio()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = "john@example.com";

        // Act
        var user = User.Create(firstName, lastName, email);

        // Assert
        user.Bio.Should().BeNull();
    }

    [Fact]
    public void Create_WithEmailAndPasswordHash_ShouldCreateUser()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = Email.Create("john@example.com");
        var passwordHash = "hashedpassword123";
        var bio = "Software Developer";

        // Act
        var user = User.Create(firstName, lastName, email, passwordHash, bio);

        // Assert
        user.Should().NotBeNull();
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Email.Should().Be(email);
        user.PasswordHash.Should().Be(passwordHash);
        user.Bio.Should().Be(bio);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidFirstName_ShouldThrowArgumentException(string invalidFirstName)
    {
        // Arrange
        var lastName = "Doe";
        var email = "john@example.com";

        // Act & Assert
        var action = () => User.Create(invalidFirstName, lastName, email);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidLastName_ShouldThrowArgumentException(string invalidLastName)
    {
        // Arrange
        var firstName = "John";
        var email = "john@example.com";

        // Act & Assert
        var action = () => User.Create(firstName, invalidLastName, email);
        action.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";

        // Act & Assert
        var action = () => User.Create(firstName, lastName, invalidEmail);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNullEmailObject_ShouldThrowArgumentNullException()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var passwordHash = "hashedpassword123";

        // Act & Assert
        var action = () => User.Create(firstName, lastName, null!, passwordHash);
        action.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidPasswordHash_ShouldThrowArgumentException(string invalidPasswordHash)
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var email = Email.Create("john@example.com");

        // Act & Assert
        var action = () => User.Create(firstName, lastName, email, invalidPasswordHash);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateProfile_WithValidData_ShouldUpdateUser()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com");
        var newFirstName = "Jane";
        var newLastName = "Smith";
        var newBio = "Product Manager";
        var newProfilePictureUrl = "https://example.com/avatar.jpg";

        // Act
        user.UpdateProfile(newFirstName, newLastName, newBio, newProfilePictureUrl);

        // Assert
        user.FirstName.Should().Be(newFirstName);
        user.LastName.Should().Be(newLastName);
        user.Bio.Should().Be(newBio);
        user.ProfilePictureUrl.Should().Be(newProfilePictureUrl);
        user.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        user.FullName.Should().Be($"{newFirstName} {newLastName}");
    }

    [Fact]
    public void UpdateEmail_WithValidEmail_ShouldUpdateEmail()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com");
        var newEmail = "newemail@example.com";

        // Act
        user.UpdateEmail(newEmail);

        // Assert
        user.Email.Value.Should().Be(newEmail.ToLowerInvariant());
        user.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void UpdateEmail_WithInvalidEmail_ShouldThrowArgumentException(string invalidEmail)
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com");

        // Act & Assert
        var action = () => user.UpdateEmail(invalidEmail);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void SetPrimaryRole_WithValidRole_ShouldSetRole()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com");
        var role = new Role { Name = "Admin", Description = "Administrator role" };
        // Set ID for testing purposes
        role.GetType().GetProperty("Id")!.SetValue(role, 1);

        // Act
        user.SetPrimaryRole(role);

        // Assert
        user.Role.Should().Be(role);
        user.RoleId.Should().Be(role.Id);
        user.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void SetPrimaryRole_WithNullRole_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com");

        // Act & Assert
        var action = () => user.SetPrimaryRole(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com");
        user.Deactivate(); // First deactivate

        // Act
        user.Activate();

        // Assert
        user.IsActive.Should().BeTrue();
        user.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var user = User.Create("John", "Doe", "john@example.com");

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void FullName_ShouldReturnConcatenatedFirstAndLastName()
    {
        // Arrange
        var firstName = "John";
        var lastName = "Doe";
        var user = User.Create(firstName, lastName, "john@example.com");

        // Act
        var fullName = user.FullName;

        // Assert
        fullName.Should().Be($"{firstName} {lastName}");
    }
}
