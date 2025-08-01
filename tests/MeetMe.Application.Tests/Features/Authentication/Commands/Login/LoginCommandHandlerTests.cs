using AutoMapper;
using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Authentication.Commands.Login;
using MeetMe.Application.Features.Authentication.DTOs;
using MeetMe.Application.Services;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;
using Moq;
using System.Linq.Expressions;

namespace MeetMe.Application.Tests.Features.Authentication.Commands.Login;

public class LoginCommandHandlerTests
{
    private readonly Mock<IQueryRepository<User, Guid>> _mockUserQueryRepository;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockUserQueryRepository = new Mock<IQueryRepository<User, Guid>>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockMapper = new Mock<IMapper>();

        _handler = new LoginCommandHandler(
            _mockUserQueryRepository.Object,
            _mockJwtTokenService.Object,
            _mockPasswordService.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccessResult()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new LoginCommand(email, password);

        var user = User.Create("John", "Doe", Email.Create(email), "hashedPassword123", "Bio");
        var userDto = new UserDto { Id = user.Id, FirstName = "John", LastName = "Doe", Email = email };
        var token = "jwt-token";
        var refreshToken = "refresh-token";

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(user))
            .Returns(token);

        _mockJwtTokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockMapper
            .Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Success.Should().BeTrue();
        result.Value.Token.Should().Be(token);
        result.Value.RefreshToken.Should().Be(refreshToken);
        result.Value.User.Should().BeEquivalentTo(userDto);
        result.Value.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddHours(24), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Handle_WithInvalidEmailFormat_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new LoginCommand("invalid-email", "password123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email format");

        _mockUserQueryRepository.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "password123");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");

        _mockPasswordService.Verify(
            x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithIncorrectPassword_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "user@example.com";
        var password = "wrongpassword";
        var command = new LoginCommand(email, password);

        var user = User.Create("John", "Doe", Email.Create(email), "hashedPassword123", "Bio");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid email or password");

        _mockJwtTokenService.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new LoginCommand(email, password);

        var user = User.Create("John", "Doe", Email.Create(email), "hashedPassword123", "Bio");
        
        // Make user inactive using reflection
        var userType = typeof(User);
        var isActiveProperty = userType.GetProperty("IsActive");
        isActiveProperty?.SetValue(user, false);

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Account is inactive");

        _mockJwtTokenService.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "password123");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Login failed");
        result.Error.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task Handle_WhenPasswordServiceThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new LoginCommand(email, password);

        var user = User.Create("John", "Doe", Email.Create(email), "hashedPassword123", "Bio");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Throws(new Exception("Password service error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Login failed");
        result.Error.Should().Contain("Password service error");
    }

    [Fact]
    public async Task Handle_WhenJwtTokenServiceThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new LoginCommand(email, password);

        var user = User.Create("John", "Doe", Email.Create(email), "hashedPassword123", "Bio");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(user))
            .Throws(new Exception("Token generation failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Login failed");
        result.Error.Should().Contain("Token generation failed");
    }

    [Fact]
    public async Task Handle_WhenMapperThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new LoginCommand(email, password);

        var user = User.Create("John", "Doe", Email.Create(email), "hashedPassword123", "Bio");
        var token = "jwt-token";
        var refreshToken = "refresh-token";

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(user))
            .Returns(token);

        _mockJwtTokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockMapper
            .Setup(x => x.Map<UserDto>(user))
            .Throws(new Exception("Mapping failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Login failed");
        result.Error.Should().Contain("Mapping failed");
    }

    [Fact]
    public async Task Handle_ShouldCallServicesWithCorrectParameters()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new LoginCommand(email, password);

        var user = User.Create("John", "Doe", Email.Create(email), "hashedPassword123", "Bio");
        var userDto = new UserDto { Id = user.Id, FirstName = "John", LastName = "Doe", Email = email };
        var token = "jwt-token";
        var refreshToken = "refresh-token";

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(user))
            .Returns(token);

        _mockJwtTokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockMapper
            .Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockPasswordService.Verify(
            x => x.VerifyPassword(password, user.PasswordHash),
            Times.Once);

        _mockJwtTokenService.Verify(
            x => x.GenerateToken(user),
            Times.Once);

        _mockJwtTokenService.Verify(
            x => x.GenerateRefreshToken(),
            Times.Once);

        _mockMapper.Verify(
            x => x.Map<UserDto>(user),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleCancellationToken()
    {
        // Arrange
        var email = "user@example.com";
        var password = "password123";
        var command = new LoginCommand(email, password);
        var cancellationToken = new CancellationToken(true);

        var user = User.Create("John", "Doe", Email.Create(email), "hashedPassword123", "Bio");
        var userDto = new UserDto { Id = user.Id, FirstName = "John", LastName = "Doe", Email = email };
        var token = "jwt-token";
        var refreshToken = "refresh-token";

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(user))
            .Returns(token);

        _mockJwtTokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockMapper
            .Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockUserQueryRepository.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email+tag@domain.co.uk")]
    [InlineData("user123@test-domain.org")]
    public async Task Handle_WithDifferentValidEmails_ShouldWork(string email)
    {
        // Arrange
        var password = "password123";
        var command = new LoginCommand(email, password);

        var user = User.Create("John", "Doe", Email.Create(email), "hashedPassword123", "Bio");
        var userDto = new UserDto { Id = user.Id, FirstName = "John", LastName = "Doe", Email = email };
        var token = "jwt-token";
        var refreshToken = "refresh-token";

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordService
            .Setup(x => x.VerifyPassword(password, user.PasswordHash))
            .Returns(true);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(user))
            .Returns(token);

        _mockJwtTokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockMapper
            .Setup(x => x.Map<UserDto>(user))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.User.Email.Should().Be(email);
    }
}
