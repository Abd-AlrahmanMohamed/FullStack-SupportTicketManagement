using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Infrastructure.Persistence;
using SupportTickets.Infrastructure.Persistence.Seed;
using Xunit;

namespace SupportTickets.Tests.Integration;

/// <summary>
/// Boots the real API pipeline (auth, authorization, middleware, MediatR) against a
/// private in-memory SQLite database instead of SQL Server, so integration tests are
/// self-contained and don't require a running database server.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "integration-test-only-secret-key-must-be-at-least-32-chars",
                ["Jwt:Issuer"] = "SupportTicketManagement",
                ["Jwt:Audience"] = "SupportTicketManagementClient",
                ["Jwt:ExpiryMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        await context.Database.EnsureCreatedAsync();
        await DbSeeder.SeedAsync(context, passwordHasher);
    }

    public new async Task DisposeAsync()
    {
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
