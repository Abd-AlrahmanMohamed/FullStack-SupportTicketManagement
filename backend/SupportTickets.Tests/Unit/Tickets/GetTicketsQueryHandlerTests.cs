using FluentAssertions;
using SupportTickets.Application.Features.Tickets.Queries.GetTickets;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Infrastructure.Persistence;
using SupportTickets.Infrastructure.Repositories;
using SupportTickets.Tests.TestUtilities;
using Xunit;

namespace SupportTickets.Tests.Unit.Tickets;

public class GetTicketsQueryHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context = TestDbContextFactory.Create();

    private GetTicketsQueryHandler CreateHandler(Guid? actingUserId, UserRole actingRole)
    {
        var currentUser = new FakeCurrentUserService { UserId = actingUserId, Role = actingRole };
        return new GetTicketsQueryHandler(new GenericRepository<Ticket>(_context), currentUser, MapperFactory.Create());
    }

    private async Task<(User customerA, User customerB, User agentA)> SeedTwoCustomersAsync()
    {
        var customerA = new User { Id = Guid.NewGuid(), FullName = "Customer A", Email = "custA@x.com", Role = UserRole.Customer };
        var customerB = new User { Id = Guid.NewGuid(), FullName = "Customer B", Email = "custB@x.com", Role = UserRole.Customer };
        var agentA = new User { Id = Guid.NewGuid(), FullName = "Agent A", Email = "agentA@x.com", Role = UserRole.SupportAgent, IsActive = true };

        _context.Users.AddRange(customerA, customerB, agentA);

        _context.Tickets.AddRange(
            new Ticket { Id = Guid.NewGuid(), TicketNumber = "TKT-100001", Title = "A1", Description = "d", CustomerId = customerA.Id, AssignedAgentId = agentA.Id },
            new Ticket { Id = Guid.NewGuid(), TicketNumber = "TKT-100002", Title = "A2", Description = "d", CustomerId = customerA.Id },
            new Ticket { Id = Guid.NewGuid(), TicketNumber = "TKT-100003", Title = "B1", Description = "d", CustomerId = customerB.Id });

        await _context.SaveChangesAsync();
        return (customerA, customerB, agentA);
    }

    [Fact]
    public async Task Customer_OnlySeesOwnTickets()
    {
        var (customerA, _, _) = await SeedTwoCustomersAsync();
        var handler = CreateHandler(customerA.Id, UserRole.Customer);

        var result = await handler.Handle(new GetTicketsQuery(), default);

        result.TotalCount.Should().Be(2);
        result.Items.Should().OnlyContain(t => t.CustomerId == customerA.Id);
    }

    [Fact]
    public async Task SupportAgent_OnlySeesAssignedTickets()
    {
        var (_, _, agentA) = await SeedTwoCustomersAsync();
        var handler = CreateHandler(agentA.Id, UserRole.SupportAgent);

        var result = await handler.Handle(new GetTicketsQuery(), default);

        result.TotalCount.Should().Be(1);
        result.Items.Should().OnlyContain(t => t.AssignedAgentId == agentA.Id);
    }

    [Fact]
    public async Task Admin_SeesAllTickets()
    {
        await SeedTwoCustomersAsync();
        var handler = CreateHandler(Guid.NewGuid(), UserRole.Admin);

        var result = await handler.Handle(new GetTicketsQuery(), default);

        result.TotalCount.Should().Be(3);
    }

    public void Dispose() => _context.Dispose();
}
