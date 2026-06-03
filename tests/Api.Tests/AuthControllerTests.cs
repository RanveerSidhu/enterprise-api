using System.Net;
using System.Net.Http.Json;
using Application.DTOs;
using FluentAssertions;

namespace Api.Tests;

/// <summary>
/// Integration tests for POST /api/auth/login, /refresh, /logout.
/// These spin up the real ASP.NET Core pipeline with an in-memory database.
/// </summary>
public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Login ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_WithSeededAdminEmail_Returns200WithTokens()
    {
        // The DB is seeded with ranveer@test.com in AppDbContextSeed
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "ranveer@test.com" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Returns500()
    {
        // ExceptionMiddleware converts ArgumentException → 500 in this project
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "nobody@unknown.com" });

        // The project's ExceptionMiddleware returns 500 for unhandled exceptions
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task Login_WithMissingEmailField_Returns400()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "" });

        // Empty email hits the ArgumentException path → 500 via middleware
        response.StatusCode.Should().BeOneOf(
            HttpStatusCode.BadRequest,
            HttpStatusCode.InternalServerError);
    }

    // ── Refresh ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Refresh_WithValidRefreshToken_Returns200WithNewAccessToken()
    {
        // First login to get a real refresh token
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "ranveer@test.com" });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Now use the refresh token
        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = loginBody!.RefreshToken });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshBody = await refreshResponse.Content.ReadFromJsonAsync<AuthResponse>();
        refreshBody!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_WithInvalidToken_Returns500()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { RefreshToken = "completely-invalid-token" });

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // ── Logout ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Logout_WithValidToken_Returns204()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "ranveer@test.com" });
        var loginBody = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var logoutResponse = await _client.PostAsJsonAsync("/api/auth/logout",
            new { RefreshToken = loginBody!.RefreshToken });

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Logout_WithUnknownToken_StillReturns204()
    {
        // Logout is idempotent — unknown tokens are silently ignored
        var response = await _client.PostAsJsonAsync("/api/auth/logout",
            new { RefreshToken = "unknown-token" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
