using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Application.DTOs;
using FluentAssertions;

namespace Api.Tests;

/// <summary>
/// Integration tests for GET/POST/PUT/DELETE /api/users.
/// All endpoints require a valid Admin JWT — tests obtain one via /api/auth/login.
/// </summary>
public class UsersControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UsersControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>Login with the seeded admin user and attach the JWT to the client.</summary>
    private async Task AuthenticateAsync()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new { Email = "ranveer@test.com" });

        loginResponse.EnsureSuccessStatusCode();

        var body = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.AccessToken);
    }

    // ── GET /api/users ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetAll_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAll_WithAdminToken_Returns200AndUsers()
    {
        await AuthenticateAsync();

        var response = await _client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
        users.Should().NotBeNull();
        users!.Count.Should().BeGreaterThan(0);
    }

    // ── POST /api/users ──────────────────────────────────────────────────────

    [Fact]
    public async Task CreateUser_WithValidData_Returns201AndNewUser()
    {
        await AuthenticateAsync();

        var request = new CreateUserRequest
        {
            FullName = "Jane Test",
            Email    = "jane.test@example.com"
        };

        var response = await _client.PostAsJsonAsync("/api/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await response.Content.ReadFromJsonAsync<UserDto>();
        created.Should().NotBeNull();
        created!.FullName.Should().Be("Jane Test");
        created.Email.Should().Be("jane.test@example.com");
        created.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUser_WithEmptyFullName_Returns500()
    {
        await AuthenticateAsync();

        var request = new CreateUserRequest { FullName = "", Email = "valid@example.com" };

        var response = await _client.PostAsJsonAsync("/api/users", request);

        // ExceptionMiddleware converts ArgumentException → 500
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task CreateUser_WithoutToken_Returns401()
    {
        _client.DefaultRequestHeaders.Authorization = null;

        var request = new CreateUserRequest { FullName = "Test", Email = "t@t.com" };
        var response = await _client.PostAsJsonAsync("/api/users", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── PUT /api/users/{id} ──────────────────────────────────────────────────

    [Fact]
    public async Task UpdateUser_WithValidData_Returns200AndUpdatedUser()
    {
        await AuthenticateAsync();

        // First create a user to update
        var createRequest = new CreateUserRequest
        {
            FullName = "Original Name",
            Email    = "original@example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/users", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        // Now update it
        var updateRequest = new UpdateUserRequest
        {
            FullName = "Updated Name",
            Email    = "updated@example.com"
        };
        var updateResponse = await _client.PutAsJsonAsync($"/api/users/{created!.Id}", updateRequest);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<UserDto>();
        updated!.FullName.Should().Be("Updated Name");
        updated.Email.Should().Be("updated@example.com");
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentId_Returns500()
    {
        await AuthenticateAsync();

        var request = new UpdateUserRequest { FullName = "Name", Email = "e@e.com" };
        var response = await _client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    // ── DELETE /api/users/{id} ───────────────────────────────────────────────

    [Fact]
    public async Task DeleteUser_WithValidId_Returns204()
    {
        await AuthenticateAsync();

        // Create a user then delete it
        var createRequest = new CreateUserRequest
        {
            FullName = "To Delete",
            Email    = "delete.me@example.com"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/users", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<UserDto>();

        var deleteResponse = await _client.DeleteAsync($"/api/users/{created!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteUser_WithNonExistentId_Returns500()
    {
        await AuthenticateAsync();

        var response = await _client.DeleteAsync($"/api/users/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }
}
