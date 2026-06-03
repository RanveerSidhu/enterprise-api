using Application.DTOs;
using Application.Services;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace Application.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();
        _sut = new UserService(_repoMock.Object);
    }

    // ── GetAllUsers ──────────────────────────────────────────────────────────

    [Fact]
    public void GetAllUsers_ReturnsAllUsers_AsDtos()
    {
        // Arrange
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), FullName = "Alice Smith", Email = "alice@example.com" },
            new() { Id = Guid.NewGuid(), FullName = "Bob Jones",  Email = "bob@example.com"  }
        };
        _repoMock.Setup(r => r.GetAll()).Returns(users);

        // Act
        var result = _sut.GetAllUsers().ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].FullName.Should().Be("Alice Smith");
        result[1].Email.Should().Be("bob@example.com");
    }

    [Fact]
    public void GetAllUsers_WhenNoUsers_ReturnsEmptyList()
    {
        _repoMock.Setup(r => r.GetAll()).Returns(new List<User>());

        var result = _sut.GetAllUsers();

        result.Should().BeEmpty();
    }

    // ── GetUserById ──────────────────────────────────────────────────────────

    [Fact]
    public void GetUserById_WhenUserExists_ReturnsDto()
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id, FullName = "Alice Smith", Email = "alice@example.com" };
        _repoMock.Setup(r => r.GetById(id)).Returns(user);

        var result = _sut.GetUserById(id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.FullName.Should().Be("Alice Smith");
    }

    [Fact]
    public void GetUserById_WhenUserNotFound_ReturnsNull()
    {
        _repoMock.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((User?)null);

        var result = _sut.GetUserById(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── CreateUser ───────────────────────────────────────────────────────────

    [Fact]
    public void CreateUser_WithValidRequest_ReturnsCreatedDto()
    {
        var request = new CreateUserRequest { FullName = "Charlie Brown", Email = "charlie@example.com" };
        _repoMock.Setup(r => r.Add(It.IsAny<User>()));

        var result = _sut.CreateUser(request);

        result.Should().NotBeNull();
        result.FullName.Should().Be("Charlie Brown");
        result.Email.Should().Be("charlie@example.com");
        result.Id.Should().NotBeEmpty();
        _repoMock.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public void CreateUser_WhenFullNameIsEmpty_ThrowsArgumentException()
    {
        var request = new CreateUserRequest { FullName = "", Email = "test@example.com" };

        Action act = () => _sut.CreateUser(request);

        act.Should().Throw<ArgumentException>().WithMessage("*Full name*");
    }

    [Fact]
    public void CreateUser_WhenEmailIsEmpty_ThrowsArgumentException()
    {
        var request = new CreateUserRequest { FullName = "Valid Name", Email = "" };

        Action act = () => _sut.CreateUser(request);

        act.Should().Throw<ArgumentException>().WithMessage("*Email*");
    }

    [Fact]
    public void CreateUser_WhenFullNameIsWhitespace_ThrowsArgumentException()
    {
        var request = new CreateUserRequest { FullName = "   ", Email = "test@example.com" };

        Action act = () => _sut.CreateUser(request);

        act.Should().Throw<ArgumentException>();
    }

    // ── UpdateUser ───────────────────────────────────────────────────────────

    [Fact]
    public void UpdateUser_WhenUserExists_UpdatesAndReturnsDto()
    {
        var id = Guid.NewGuid();
        var existing = new User { Id = id, FullName = "Old Name", Email = "old@example.com" };
        var request = new UpdateUserRequest { FullName = "New Name", Email = "new@example.com" };

        _repoMock.Setup(r => r.GetById(id)).Returns(existing);
        _repoMock.Setup(r => r.Update(It.IsAny<User>()));

        var result = _sut.UpdateUser(id, request);

        result.FullName.Should().Be("New Name");
        result.Email.Should().Be("new@example.com");
        _repoMock.Verify(r => r.Update(It.Is<User>(u => u.FullName == "New Name")), Times.Once);
    }

    [Fact]
    public void UpdateUser_WhenUserNotFound_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((User?)null);
        var request = new UpdateUserRequest { FullName = "Name", Email = "email@example.com" };

        Action act = () => _sut.UpdateUser(Guid.NewGuid(), request);

        act.Should().Throw<KeyNotFoundException>().WithMessage("*User not found*");
    }

    [Fact]
    public void UpdateUser_WhenFullNameIsEmpty_ThrowsArgumentException()
    {
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetById(id)).Returns(new User { Id = id, FullName = "X", Email = "x@x.com" });
        var request = new UpdateUserRequest { FullName = "", Email = "valid@example.com" };

        Action act = () => _sut.UpdateUser(id, request);

        act.Should().Throw<ArgumentException>().WithMessage("*Full name*");
    }

    // ── DeleteUser ───────────────────────────────────────────────────────────

    [Fact]
    public void DeleteUser_WhenUserExists_CallsRepositoryDelete()
    {
        var id = Guid.NewGuid();
        var user = new User { Id = id, FullName = "Alice", Email = "alice@example.com" };
        _repoMock.Setup(r => r.GetById(id)).Returns(user);

        _sut.DeleteUser(id);

        _repoMock.Verify(r => r.Delete(user), Times.Once);
    }

    [Fact]
    public void DeleteUser_WhenUserNotFound_ThrowsKeyNotFoundException()
    {
        _repoMock.Setup(r => r.GetById(It.IsAny<Guid>())).Returns((User?)null);

        Action act = () => _sut.DeleteUser(Guid.NewGuid());

        act.Should().Throw<KeyNotFoundException>().WithMessage("*User not found*");
    }
}
