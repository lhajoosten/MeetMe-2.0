using FluentAssertions;
using MeetMe.Application.Features.Users.Commands.CreateUser;

namespace MeetMe.Application.Tests.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator;

    public CreateUserCommandValidatorTests()
    {
        _validator = new CreateUserCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Bio = "Software developer with 5 years of experience"
        };

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
    public void Validate_WithInvalidFirstName_ShouldFail(string firstName)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = firstName,
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.FirstName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "First name is required");
    }

    [Fact]
    public void Validate_WithTooLongFirstName_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = new string('A', 101), // 101 characters
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.FirstName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "First name must not exceed 100 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidLastName_ShouldFail(string lastName)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = lastName,
            Email = "john.doe@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.LastName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Last name is required");
    }

    [Fact]
    public void Validate_WithTooLongLastName_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = new string('B', 101), // 101 characters
            Email = "john.doe@example.com"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.LastName));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Last name must not exceed 100 characters");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithInvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = email
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
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
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = email
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
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
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = email
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithTooLongEmail_ShouldFail()
    {
        // Arrange
        var longEmail = new string('a', 312) + "@example.com"; // > 320 characters
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = longEmail
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Email must not exceed 320 characters");
    }

    [Fact]
    public void Validate_WithNullBio_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Bio = null
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyBio_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Bio = ""
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidBio_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Bio = "Experienced software developer specializing in .NET and cloud technologies."
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithTooLongBio_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Bio = new string('C', 501) // 501 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Bio));
        result.Errors.Should().Contain(e => e.ErrorMessage == "Bio must not exceed 500 characters");
    }

    [Fact]
    public void Validate_WithMaxLengthBio_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Bio = new string('C', 500) // Exactly 500 characters
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithMultipleErrors_ShouldReturnAllErrors()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "",
            LastName = "",
            Email = "invalid-email",
            Bio = new string('D', 501)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(4); // FirstName, LastName, Email format, Bio length
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.FirstName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.LastName));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Bio));
    }
}
