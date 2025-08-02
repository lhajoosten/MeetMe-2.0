using FluentAssertions;
using MeetMe.Application.Features.Authentication.Commands.Register;

namespace MeetMe.Application.Tests.Features.Authentication.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidEmail_ShouldFail(string? email)
    {
        // Arrange
        var command = new RegisterCommand(
            email!, 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email is required");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user.example.com")]
    [InlineData("user@@example.com")]
    public void Validate_WithInvalidEmailFormat_ShouldFail(string email)
    {
        // Arrange
        var command = new RegisterCommand(
            email, 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid email format");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidPassword_ShouldFail(string? password)
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            password!, 
            password!, 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Password));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required");
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("Pass")]
    [InlineData("a")]
    public void Validate_WithTooShortPassword_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            password, 
            password, 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Password));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must be at least 6 characters long");
    }

    [Theory]
    [InlineData("password123")] // no uppercase
    [InlineData("PASSWORD123")] // no lowercase
    [InlineData("Password")] // no number
    [InlineData("123456")] // no letters
    [InlineData("abcdef")] // no uppercase or number
    public void Validate_WithWeakPassword_ShouldFail(string password)
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            password, 
            password, 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Password));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password must contain at least one uppercase letter, one lowercase letter, and one number");
    }

    [Theory]
    [InlineData("Password123")]
    [InlineData("MyPassword1")]
    [InlineData("SecurePass99")]
    public void Validate_WithValidPassword_ShouldPass(string password)
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            password, 
            password, 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidConfirmPassword_ShouldFail(string? confirmPassword)
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            confirmPassword!, 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.ConfirmPassword));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Confirm password is required");
    }

    [Fact]
    public void Validate_WithNonMatchingPasswords_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "DifferentPassword123", 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.ConfirmPassword));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Passwords do not match");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidFirstName_ShouldFail(string? firstName)
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            firstName!, 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.FirstName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "First name is required");
    }

    [Fact]
    public void Validate_WithTooLongFirstName_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            new string('A', 51), // 51 characters
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.FirstName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "First name cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidLastName_ShouldFail(string? lastName)
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            lastName!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.LastName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Last name is required");
    }

    [Fact]
    public void Validate_WithTooLongLastName_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            new string('B', 51)); // 51 characters

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.LastName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Last name cannot exceed 50 characters");
    }

    [Fact]
    public void Validate_WithMaxLengthNames_ShouldPass()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            new string('A', 50), // exactly 50 characters
            new string('B', 50)); // exactly 50 characters

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new RegisterCommand("", "", "", "", "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(8); // Email, Password, ConfirmPassword, FirstName, LastName
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.Password));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.ConfirmPassword));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.FirstName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(RegisterCommand.LastName));
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email+tag@domain.co.uk")]
    [InlineData("user123@test-domain.org")]
    [InlineData("firstname.lastname@company.com")]
    public void Validate_WithValidEmailFormats_ShouldPass(string email)
    {
        // Arrange
        var command = new RegisterCommand(
            email, 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
