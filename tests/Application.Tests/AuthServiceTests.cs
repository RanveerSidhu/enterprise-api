using Application.DTOs;
using Application.Services;
using Domain.Entities;
using Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _repoMock;
    private readonly IConfiguration _config;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _repoMock = new Mock<IUserRepository>();

        // Minimal JWT config — mirrors what appsettings.json provides
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Jwt:Key"]      = "SuperSecretTestKey_AtLeast32CharactersLong!",
            ["Jwt:Issuer"]   = "TestIssuer",
            ["Jwt:Audience"] = "TestAudience"
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _sut = new AuthService(_config, _repoMock.Object);
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public void Login_WithValidEmail_ReturnsTokens()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "alice@example.com", Role = "User" };
        _repoMock.Setup(r => r.GetByEmail("alice@example.com")).Returns(user);
        _repoMock.Setup(r => r.Update(It.IsAny<User>()));

        var result = _sut.Login("alice@example.com");

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Login_WithValidEmail_PersistsRefreshToken()
    {
        var user = new User { Id = Guid.NewGuid(), Email = "alice@example.com", Role = "User" };
        _repoMock.Setup(r => r.GetByEmail("alice@example.com")).Returns(user);

        _sut.Login("alice@example.com");

        _repoMock.Verify(r => r.Update(It.Is<User>(u =>
            u.RefreshToken != null &&
            u.RefreshTokenExpiry > DateTime.UtcNow
        )), Times.Once);
    }

    [Fact]
    public void Login_WithUnknownEmail_ThrowsArgumentException()
    {
        _repoMock.Setup(r => r.GetByEmail(It.IsAny<string>())).Returns((User?)null);

        Action act = () => _sut.Login("unknown@example.com");

        act.Should().Throw<ArgumentException>().WithMessage("*Invalid email*");
    }

    // ── RefreshToken ─────────────────────────────────────────────────────────

    [Fact]
    public void RefreshToken_WithValidToken_ReturnsNewAccessToken()
    {
        var refreshToken = Guid.NewGuid().ToString();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "alice@example.com",
            Role = "User",
            RefreshToken = refreshToken,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
        };
        _repoMock.Setup(r => r.GetByRefreshToken(refreshToken)).Returns(user);

        var result = _sut.RefreshToken(refreshToken);

        result.AccessToken.Should().NotBeNullOrWhiteSpace();
        result.RefreshToken.Should().Be(refreshToken);
    }

    [Fact]
    public void RefreshToken_WithExpiredToken_ThrowsArgumentException()
    {
        var refreshToken = "expired-token";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "alice@example.com",
            Role = "User",
            RefreshToken = refreshToken,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(-1)  // already expired
        };
        _repoMock.Setup(r => r.GetByRefreshToken(refreshToken)).Returns(user);

        Action act = () => _sut.RefreshToken(refreshToken);

        act.Should().Throw<ArgumentException>().WithMessage("*expired*");
    }

    [Fact]
    public void RefreshToken_WithInvalidToken_ThrowsArgumentException()
    {
        _repoMock.Setup(r => r.GetByRefreshToken(It.IsAny<string>())).Returns((User?)null);

        Action act = () => _sut.RefreshToken("bad-token");

        act.Should().Throw<ArgumentException>().WithMessage("*Invalid refresh token*");
    }

    // ── Logout ───────────────────────────────────────────────────────────────

    [Fact]
    public void Logout_WithValidToken_ClearsRefreshToken()
    {
        var refreshToken = "valid-token";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "alice@example.com",
            Role = "User",
            RefreshToken = refreshToken,
            RefreshTokenExpiry = DateTime.UtcNow.AddDays(7)
        };
        _repoMock.Setup(r => r.GetByRefreshToken(refreshToken)).Returns(user);

        _sut.Logout(refreshToken);

        _repoMock.Verify(r => r.Update(It.Is<User>(u =>
            u.RefreshToken == null &&
            u.RefreshTokenExpiry == null
        )), Times.Once);
    }

    [Fact]
    public void Logout_WithUnknownToken_DoesNotThrow()
    {
        _repoMock.Setup(r => r.GetByRefreshToken(It.IsAny<string>())).Returns((User?)null);

        Action act = () => _sut.Logout("unknown-token");

        act.Should().NotThrow();
        _repoMock.Verify(r => r.Update(It.IsAny<User>()), Times.Never);
    }
}
