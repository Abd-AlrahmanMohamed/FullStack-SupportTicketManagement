using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Infrastructure.Persistence;

namespace SupportTickets.Tests.TestUtilities;

/// <summary>
/// Creates a real ApplicationDbContext backed by a private in-memory SQLite database.
/// Used so handler tests exercise the real GenericRepository/EF Core query pipeline
/// (async LINQ, Include, etc.) instead of a hand-rolled fake that can't support it.
/// </summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
