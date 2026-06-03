using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Api.Tests;

/// <summary>
/// Boots the real ASP.NET Core pipeline but replaces the SQL Server
/// DbContext with an EF Core in-memory database.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove every descriptor whose service type or implementation
            // touches DbContext or DbContextOptions so no SqlServer
            // provider artifacts remain in the container.
            var toRemove = services
                .Where(d =>
                    d.ServiceType.FullName != null &&
                    (d.ServiceType.FullName.Contains("DbContext") ||
                     d.ServiceType.FullName.Contains("DbContextOptions")))
                .ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            // Register a fresh AppDbContext backed by InMemory only.
            // Use a fixed name so all scopes share the same in-memory store.
            services.AddDbContext<AppDbContext>(options =>
                options
                    .UseInMemoryDatabase("TestDb")
                    .ConfigureWarnings(w =>
                        w.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });

        builder.UseSetting("Jwt:Key", "SuperSecretTestKey_AtLeast32CharactersLong!");
        builder.UseSetting("Jwt:Issuer", "TestIssuer");
        builder.UseSetting("Jwt:Audience", "TestAudience");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "InMemory");
        builder.UseEnvironment("Development");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Seed the in-memory database after the host is fully built.
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        AppDbContextSeed.Seed(db);

        return host;
    }
}