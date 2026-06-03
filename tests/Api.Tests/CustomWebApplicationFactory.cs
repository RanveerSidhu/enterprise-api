using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

/// <summary>
/// Boots the real ASP.NET Core pipeline but swaps SQL Server for an
/// in-memory EF Core database — no real DB needed to run these tests.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the real SQL Server DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add in-memory database (unique name per factory instance)
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        });

        // Override JWT and connection string config for tests
        builder.UseSetting("Jwt:Key",      "SuperSecretTestKey_AtLeast32CharactersLong!");
        builder.UseSetting("Jwt:Issuer",   "TestIssuer");
        builder.UseSetting("Jwt:Audience", "TestAudience");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "InMemory");

        builder.UseEnvironment("Development");
    }
}
