using FluentAssertions;
using MeetMe.Application.Common.Interfaces;
using MeetMe.Application.Common.Models;
using MeetMe.Application.Features.Users.Queries.GetUser;
using MeetMe.Domain.Entities;
using Moq;

namespace MeetMe.Application.Tests.Features.Users.Queries.GetUser;

public class GetUserByIdQueryHandlerTests
{
    private readonly Mock<IQueryRepository<User, Guid>> _userRepositoryMock;
    private readonly GetUserByIdQueryHandler _handler;

    public GetUserByIdQueryHandlerTests()
    {
        _userRepositoryMock = new Mock<IQueryRepository<User, Guid>>();
        _handler = new GetUserByIdQueryHandler(_userRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingUser_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        
        var user = User.Create("John", "Doe", "john.doe@example.com");
        var userType = typeof(User);
        
        // Set private properties using reflection for testing
        userType.GetProperty("Id")?.SetValue(user, userId);
        userType.GetProperty("Bio")?.SetValue(user, "Software developer");
        userType.GetProperty("ProfilePictureUrl")?.SetValue(user, "https://example.com/profile.jpg");
        userType.GetProperty("IsActive")?.SetValue(user, true);
        userType.GetProperty("CreatedDate")?.SetValue(user, DateTime.UtcNow.AddDays(-30));

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user!);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(userId);
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.FullName.Should().Be("John Doe");
        result.Value.Email.Should().Be("john.doe@example.com");
        result.Value.Bio.Should().Be("Software developer");
        result.Value.ProfilePictureUrl.Should().Be("https://example.com/profile.jpg");
        result.Value.IsActive.Should().BeTrue();
        result.Value.CreatedDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(-30), TimeSpan.FromMinutes(1));

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Failed to get user");
        result.Error.Should().Contain("Database connection failed");
    }

    [Fact]
    public async Task Handle_WithUserWithoutBio_ShouldReturnUserDtoWithNullBio()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        
        var user = User.Create("Jane", "Smith", "jane.smith@example.com");
        var userType = typeof(User);
        
        // Set properties with Bio as null
        userType.GetProperty("Id")?.SetValue(user, userId);
        userType.GetProperty("Bio")?.SetValue(user, null);
        userType.GetProperty("ProfilePictureUrl")?.SetValue(user, null);
        userType.GetProperty("IsActive")?.SetValue(user, true);
        userType.GetProperty("CreatedDate")?.SetValue(user, DateTime.UtcNow.AddDays(-10));

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(userId);
        result.Value.FirstName.Should().Be("Jane");
        result.Value.LastName.Should().Be("Smith");
        result.Value.FullName.Should().Be("Jane Smith");
        result.Value.Email.Should().Be("jane.smith@example.com");
        result.Value.Bio.Should().BeNull();
        result.Value.ProfilePictureUrl.Should().BeNull();
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ShouldReturnUserDtoWithCorrectStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        
        var user = User.Create("Bob", "Johnson", "bob.johnson@example.com");
        var userType = typeof(User);
        
        // Set user as inactive
        userType.GetProperty("Id")?.SetValue(user, userId);
        userType.GetProperty("IsActive")?.SetValue(user, false);
        userType.GetProperty("CreatedDate")?.SetValue(user, DateTime.UtcNow.AddDays(-60));

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsActive.Should().BeFalse();
        result.Value.FirstName.Should().Be("Bob");
        result.Value.LastName.Should().Be("Johnson");
        result.Value.Email.Should().Be("bob.johnson@example.com");
    }

    [Fact]
    public async Task Handle_WithCancellationToken_ShouldPassTokenToRepository()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var cancellationToken = new CancellationToken();
        
        var user = User.Create("Alice", "Wilson", "alice.wilson@example.com");
        var userType = typeof(User);
        userType.GetProperty("Id")?.SetValue(user, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, cancellationToken))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(userId, cancellationToken),
            Times.Once);
    }

    [Theory]
    [InlineData("Alice", "Cooper")]
    [InlineData("Bob", "Dylan")]
    [InlineData("Charlie", "Brown")]
    public async Task Handle_WithDifferentUsers_ShouldReturnCorrectFullName(string firstName, string lastName)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com";
        
        var user = User.Create(firstName, lastName, email);
        var userType = typeof(User);
        userType.GetProperty("Id")?.SetValue(user, userId);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
        result.Value.FullName.Should().Be($"{firstName} {lastName}");
        result.Value.Email.Should().Be(email);
    }

    [Fact]
    public async Task Handle_WithUserWithLongBio_ShouldReturnCompleteUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserByIdQuery(userId);
        var longBio = "Senior Software Engineer with over 10 years of experience in developing scalable web applications using .NET, C#, and cloud technologies. Passionate about clean code, architecture patterns, and mentoring junior developers.";
        
        var user = User.Create("Senior", "Developer", "senior.developer@company.com");
        var userType = typeof(User);
        
        userType.GetProperty("Id")?.SetValue(user, userId);
        userType.GetProperty("Bio")?.SetValue(user, longBio);
        userType.GetProperty("ProfilePictureUrl")?.SetValue(user, "https://cdn.example.com/profiles/senior-dev.jpg");
        userType.GetProperty("IsActive")?.SetValue(user, true);
        userType.GetProperty("CreatedDate")?.SetValue(user, DateTime.UtcNow.AddYears(-2));

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Bio.Should().Be(longBio);
        result.Value.ProfilePictureUrl.Should().Be("https://cdn.example.com/profiles/senior-dev.jpg");
        result.Value.FirstName.Should().Be("Senior");
        result.Value.LastName.Should().Be("Developer");
        result.Value.FullName.Should().Be("Senior Developer");
        result.Value.Email.Should().Be("senior.developer@company.com");
        result.Value.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldAttemptToFindUser()
    {
        // Arrange
        var emptyGuid = Guid.Empty;
        var query = new GetUserByIdQuery(emptyGuid);

        _userRepositoryMock
            .Setup(x => x.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("User not found");

        _userRepositoryMock.Verify(
            x => x.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
