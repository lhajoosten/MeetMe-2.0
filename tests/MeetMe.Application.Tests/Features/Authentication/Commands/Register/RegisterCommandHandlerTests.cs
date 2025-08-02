using AutoMapper;
using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Features.Authentication.Commands.Register;
using MeetMe.Application.Features.Authentication.DTOs;
using MeetMe.Application.Features.Users.DTOs;
using MeetMe.Application.Services;
using MeetMe.Domain.Entities;
using MeetMe.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;

namespace MeetMe.Application.Tests.Features.Authentication.Commands.Register;

public class RegisterCommandHandlerTests
{
    private readonly Mock<ICommandRepository<User, int>> _mockUserCommandRepository;
    private readonly Mock<IQueryRepository<User, int>> _mockUserQueryRepository;
    private readonly Mock<IQueryRepository<Role, int>> _mockRoleQueryRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _mockUserCommandRepository = new Mock<ICommandRepository<User, int>>();
        _mockUserQueryRepository = new Mock<IQueryRepository<User, int>>();
        _mockRoleQueryRepository = new Mock<IQueryRepository<Role, int>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockMapper = new Mock<IMapper>();

        _handler = new RegisterCommandHandler(
            _mockUserCommandRepository.Object,
            _mockUserQueryRepository.Object,
            _mockRoleQueryRepository.Object,
            _mockUnitOfWork.Object,
            _mockJwtTokenService.Object,
            _mockPasswordService.Object,
            _mockMapper.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnSuccessResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        var hashedPassword = "hashedPassword123";
        var token = "jwt-token";
        var refreshToken = "refresh-token";
        var createdUser = User.Create("John", "Doe", Email.Create("user@example.com"), hashedPassword, "Bio");
        var userDto = new UserDto 
        { 
            Id = 1, 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "user@example.com" 
        };

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(token);

        _mockJwtTokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockMapper
            .Setup(x => x.Map<UserDto>(It.IsAny<User>()))
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
        var command = new RegisterCommand(
            "invalid-email", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

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
    public async Task Handle_WithNonMatchingPasswords_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "DifferentPassword", 
            "John", 
            "Doe");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Passwords do not match");

        _mockUserQueryRepository.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithTooShortPassword_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Pass1", 
            "Pass1", 
            "John", 
            "Doe");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Password must be at least 6 characters long");

        _mockUserQueryRepository.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        var existingUser = User.Create("Jane", "Smith", Email.Create("user@example.com"), "hashedPassword", "Bio");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User with this email already exists");

        _mockPasswordService.Verify(
            x => x.HashPassword(It.IsAny<string>()),
            Times.Never);

        _mockUserCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserQueryRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Registration failed");
        result.Error.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task Handle_WhenPasswordServiceThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService
            .Setup(x => x.HashPassword(command.Password))
            .Throws(new Exception("Password hashing failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Registration failed");
        result.Error.Should().Contain("Password hashing failed");
    }

    [Fact]
    public async Task Handle_WhenUserCommandRepositoryThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        var hashedPassword = "hashedPassword123";

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Failed to add user"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Registration failed");
        result.Error.Should().Contain("Failed to add user");
    }

    [Fact]
    public async Task Handle_WhenUnitOfWorkThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        var hashedPassword = "hashedPassword123";
        var createdUser = User.Create("John", "Doe", Email.Create("user@example.com"), hashedPassword, "Bio");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save changes failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Registration failed");
        result.Error.Should().Contain("Save changes failed");
    }

    [Fact]
    public async Task Handle_WhenJwtTokenServiceThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        var hashedPassword = "hashedPassword123";
        var createdUser = User.Create("John", "Doe", Email.Create("user@example.com"), hashedPassword, "Bio");

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Throws(new Exception("Token generation failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Registration failed");
        result.Error.Should().Contain("Token generation failed");
    }

    [Fact]
    public async Task Handle_ShouldCallServicesWithCorrectParameters()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        var hashedPassword = "hashedPassword123";
        var token = "jwt-token";
        var refreshToken = "refresh-token";
        var createdUser = User.Create("John", "Doe", Email.Create("user@example.com"), hashedPassword, "Bio");
        var userDto = new UserDto 
        { 
            Id = 1,
            FirstName = "John", 
            LastName = "Doe", 
            Email = "user@example.com" 
        };

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(token);

        _mockJwtTokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockMapper
            .Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockPasswordService.Verify(
            x => x.HashPassword(command.Password),
            Times.Once);

        _mockUserCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), 0, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(0, It.IsAny<CancellationToken>()),
            Times.Once);

        _mockJwtTokenService.Verify(
            x => x.GenerateToken(It.IsAny<User>()),
            Times.Once);

        _mockJwtTokenService.Verify(
            x => x.GenerateRefreshToken(),
            Times.Once);

        _mockMapper.Verify(
            x => x.Map<UserDto>(It.IsAny<User>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHandleCancellationToken()
    {
        // Arrange
        var command = new RegisterCommand(
            "user@example.com", 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        var cancellationToken = new CancellationToken(true);
        var hashedPassword = "hashedPassword123";
        var token = "jwt-token";
        var refreshToken = "refresh-token";
        var createdUser = User.Create("John", "Doe", Email.Create("user@example.com"), hashedPassword, "Bio");
        var userDto = new UserDto 
        { 
            Id = 1, 
            FirstName = "John", 
            LastName = "Doe", 
            Email = "user@example.com" 
        };

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken))
            .ReturnsAsync((User?)null);

        _mockPasswordService
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(createdUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(1);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(token);

        _mockJwtTokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockMapper
            .Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _mockUserQueryRepository.Verify(
            x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), cancellationToken),
            Times.Once);

        _mockUserCommandRepository.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), cancellationToken),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.SaveChangesAsync(It.IsAny<int>(), cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.email+tag@domain.co.uk")]
    [InlineData("user123@test-domain.org")]
    public async Task Handle_WithDifferentValidEmails_ShouldWork(string email)
    {
        // Arrange
        var command = new RegisterCommand(
            email, 
            "Password123", 
            "Password123", 
            "John", 
            "Doe");

        var hashedPassword = "hashedPassword123";
        var token = "jwt-token";
        var refreshToken = "refresh-token";
        var createdUser = User.Create("John", "Doe", Email.Create(email), hashedPassword, "Bio");
        var userDto = new UserDto 
        { 
            Id = 1,
            FirstName = "John", 
            LastName = "Doe", 
            Email = email 
        };

        _mockUserQueryRepository
            .Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _mockPasswordService
            .Setup(x => x.HashPassword(command.Password))
            .Returns(hashedPassword);

        _mockUserCommandRepository
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        _mockUnitOfWork
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockJwtTokenService
            .Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns(token);

        _mockJwtTokenService
            .Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);

        _mockMapper
            .Setup(x => x.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.User.Email.Should().Be(email);
    }
}
