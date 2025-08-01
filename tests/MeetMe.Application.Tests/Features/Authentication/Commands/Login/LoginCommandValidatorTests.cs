using FluentAssertions;
using MeetMe.Application.Features.Authentication.Commands.Login;

namespace MeetMe.Application.Tests.Features.Authentication.Commands.Login;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "password123");

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
    public void Validate_WithInvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new LoginCommand(email!, "password123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
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
        var command = new LoginCommand(email, "password123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid email format");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email+tag@domain.co.uk")]
    [InlineData("user123@test-domain.org")]
    [InlineData("firstname.lastname@company.com")]
    public void Validate_WithValidEmailFormats_ShouldPass(string email)
    {
        // Arrange
        var command = new LoginCommand(email, "password123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidPassword_ShouldFail(string password)
    {
        // Arrange
        var command = new LoginCommand("user@example.com", password!);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required");
    }

    [Theory]
    [InlineData("password")]
    [InlineData("123")]
    [InlineData("a")]
    [InlineData("verylongpasswordthatshouldbefine")]
    public void Validate_WithAnyNonEmptyPassword_ShouldPass(string password)
    {
        // Arrange
        var command = new LoginCommand("user@example.com", password);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new LoginCommand("", "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(3);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
    }

    [Fact]
    public void Validate_WithInvalidEmailAndValidPassword_ShouldFailOnEmailOnly()
    {
        // Arrange
        var command = new LoginCommand("invalid-email", "password123");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Invalid email format");
    }

    [Fact]
    public void Validate_WithValidEmailAndInvalidPassword_ShouldFailOnPasswordOnly()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Should().Contain(e => e.PropertyName == nameof(LoginCommand.Password));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Password is required");
    }
}
