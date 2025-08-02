using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Features.Users.Commands.CreateUser;
using MeetMe.Domain.Entities;
using Moq;

namespace MeetMe.Application.Tests.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IQueryRepository<User, int>> _userQueryRepositoryMock;
    private readonly Mock<ICommandRepository<User, int>> _userCommandRepositoryMock;
    private readonly CreateUserCommandHandler _handler;

    public CreateUserCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userQueryRepositoryMock = new Mock<IQueryRepository<User, int>>();
        _userCommandRepositoryMock = new Mock<ICommandRepository<User, int>>();

        _unitOfWorkMock
            .Setup(x => x.CommandRepository<User, int>())
            .Returns(_userCommandRepositoryMock.Object);

        _handler = new CreateUserCommandHandler(
            _unitOfWorkMock.Object,
            _userQueryRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateUserAndReturnId()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Bio = "Software developer"
        };

        _userQueryRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userCommandRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, string userId, CancellationToken ct) => user);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        
        _userQueryRepositoryMock.Verify(
            x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userCommandRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<User>(u =>
                    u.FirstName == command.FirstName &&
                    u.LastName == command.LastName &&
                    u.Email.Value == command.Email),
                0,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(0, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "existing@example.com"
        };

        var existingUser = User.Create("Jane", "Smith", "existing@example.com");

        _userQueryRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("A user with this email already exists");

        _userQueryRepositoryMock.Verify(
            x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _userCommandRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _userQueryRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userCommandRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create user");
        result.Error.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task Handle_WithSaveChangesException_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _userQueryRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userCommandRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, string userId, CancellationToken ct) => user);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Transaction failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create user");
        result.Error.Should().Contain("Transaction failed");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepositories()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        var cancellationToken = new CancellationToken();

        _userQueryRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                cancellationToken))
            .ReturnsAsync((User?)null);

        _userCommandRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), cancellationToken))
            .ReturnsAsync((User user, string userId, CancellationToken ct) => user);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), cancellationToken))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _userQueryRepositoryMock.Verify(
            x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                cancellationToken),
            Times.Once);

        _userCommandRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), 0, cancellationToken),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(0, cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData("Alice", "Johnson", "alice.johnson@company.com")]
    [InlineData("Bob", "Williams", "bob.williams@test.org")]
    [InlineData("Charlie", "Brown", "charlie.brown@domain.net")]
    public async Task Handle_WithDifferentValidUsers_ShouldCreateSuccessfully(
        string firstName, string lastName, string email)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email
        };

        _userQueryRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userCommandRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, string userId, CancellationToken ct) => user);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _userCommandRepositoryMock.Verify(
            x => x.AddAsync(
                It.Is<User>(u =>
                    u.FirstName == firstName &&
                    u.LastName == lastName &&
                    u.Email.Value == email),
                0,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullBio_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Bio = null
        };

        _userQueryRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userCommandRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, string userId, CancellationToken ct) => user);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithQueryRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com"
        };

        _userQueryRepositoryMock
            .Setup(x => x.FirstOrDefaultAsync(
                It.IsAny<System.Linq.Expressions.Expression<System.Func<User, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Query execution failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to create user");
        result.Error.Should().Contain("Query execution failed");

        _userCommandRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<User>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
