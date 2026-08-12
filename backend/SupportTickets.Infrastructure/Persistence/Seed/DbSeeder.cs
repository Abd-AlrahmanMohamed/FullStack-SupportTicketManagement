using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Infrastructure.Persistence.Seed;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var admin = new User
        {
            FullName = "Alice Admin",
            Email = "admin@support.local",
            PasswordHash = passwordHasher.HashPassword("Admin@123"),
            Role = UserRole.Admin,
            IsActive = true
        };

        var agent = new User
        {
            FullName = "Bob Agent",
            Email = "agent@support.local",
            PasswordHash = passwordHasher.HashPassword("Agent@123"),
            Role = UserRole.SupportAgent,
            IsActive = true
        };

        var agent2 = new User
        {
            FullName = "Carol Agent",
            Email = "agent2@support.local",
            PasswordHash = passwordHasher.HashPassword("Agent@123"),
            Role = UserRole.SupportAgent,
            IsActive = true
        };

        var customer = new User
        {
            FullName = "Dave Customer",
            Email = "customer@support.local",
            PasswordHash = passwordHasher.HashPassword("Customer@123"),
            Role = UserRole.Customer,
            IsActive = true
        };

        var customer2 = new User
        {
            FullName = "Erin Customer",
            Email = "customer2@support.local",
            PasswordHash = passwordHasher.HashPassword("Customer@123"),
            Role = UserRole.Customer,
            IsActive = true
        };

        await context.Users.AddRangeAsync(admin, agent, agent2, customer, customer2);

        var tickets = new List<Ticket>
        {
            new()
            {
                TicketNumber = "TKT-000001",
                Title = "Cannot log in to my account",
                Description = "I get an 'invalid credentials' error even though my password is correct.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.High,
                CustomerId = customer.Id
            },
            new()
            {
                TicketNumber = "TKT-000002",
                Title = "Invoice shows the wrong amount",
                Description = "My latest invoice charged me twice for the same subscription.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.Medium,
                CustomerId = customer.Id,
                AssignedAgentId = agent.Id
            },
            new()
            {
                TicketNumber = "TKT-000003",
                Title = "Feature request: dark mode",
                Description = "It would be great to have a dark theme option in the settings page.",
                Status = TicketStatus.Resolved,
                Priority = TicketPriority.Low,
                CustomerId = customer.Id,
                AssignedAgentId = agent.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                ResolvedAt = DateTime.UtcNow.AddDays(-1)
            },
            new()
            {
                TicketNumber = "TKT-000004",
                Title = "Production outage - site is down",
                Description = "The whole application is returning 500 errors for all users.",
                Status = TicketStatus.Open,
                Priority = TicketPriority.Critical,
                CustomerId = customer2.Id
            },
            new()
            {
                TicketNumber = "TKT-000005",
                Title = "Export to CSV is missing columns",
                Description = "The exported CSV file does not include the 'Status' column.",
                Status = TicketStatus.InProgress,
                Priority = TicketPriority.Medium,
                CustomerId = customer2.Id,
                AssignedAgentId = agent2.Id
            }
        };

        await context.Tickets.AddRangeAsync(tickets);

        var activities = tickets.Select(t => new TicketActivity
        {
            TicketId = t.Id,
            UserId = t.CustomerId,
            Action = "TicketCreated",
            NewValue = TicketStatus.Open.ToString()
        }).ToList();

        await context.TicketActivities.AddRangeAsync(activities);

        var loggedTicket = tickets[1];
        await context.TimeEntries.AddAsync(new TimeEntry
        {
            TicketId = loggedTicket.Id,
            AgentId = agent.Id,
            WorkDate = DateTime.UtcNow.AddDays(-1),
            DurationMinutes = 45,
            Description = "Investigated billing discrepancy in invoice service."
        });

        await context.SaveChangesAsync();
    }
}
