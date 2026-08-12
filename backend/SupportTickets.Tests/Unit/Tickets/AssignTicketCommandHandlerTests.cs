using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Features.Tickets.Commands.AssignTicket;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Infrastructure.Persistence;
using SupportTickets.Infrastructure.Repositories;
using SupportTickets.Tests.TestUtilities;
using Xunit;

namespace SupportTickets.Tests.Unit.Tickets;

public class AssignTicketCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context = TestDbContextFactory.Create();

    private AssignTicketCommandHandler CreateHandler(Guid actingAdminId)
    {
        var currentUser = new FakeCurrentUserService { UserId = actingAdminId, Role = UserRole.Admin };

        return new AssignTicketCommandHandler(
            new GenericRepository<Ticket>(_context),
            new GenericRepository<User>(_context),
            new GenericRepository<TicketActivity>(_context),
            new UnitOfWork(_context),
            currentUser,
            MapperFactory.Create(),
            NullLogger<AssignTicketCommandHandler>.Instance);
    }

    [Fact]
    public async Task Assign_ActiveSupportAgent_UpdatesTicketAndRecordsActivity()
    {
        var admin = new User { Id = Guid.NewGuid(), FullName = "Admin", Email = "adm1@x.com", Role = UserRole.Admin };
        var customer = new User { Id = Guid.NewGuid(), FullName = "Cust", Email = "c1@x.com", Role = UserRole.Customer };
        var agent = new User { Id = Guid.NewGuid(), FullName = "Agent Smith", Email = "a1@x.com", Role = UserRole.SupportAgent, IsActive = true };
        var ticket = new Ticket { Id = Guid.NewGuid(), TicketNumber = "TKT-000001", Title = "t", Description = "d", CustomerId = customer.Id, Status = TicketStatus.Open };

        _context.Users.AddRange(admin, customer, agent);
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        var handler = CreateHandler(admin.Id);

        var result = await handler.Handle(new AssignTicketCommand { TicketId = ticket.Id, AgentId = agent.Id }, default);

        result.AssignedAgentId.Should().Be(agent.Id);
        _context.Tickets.Single(t => t.Id == ticket.Id).AssignedAgentId.Should().Be(agent.Id);
        _context.TicketActivities.Should().ContainSingle(a => a.Action == "TicketAssigned");
    }

    [Fact]
    public async Task Assign_UserThatIsNotAnActiveSupportAgent_ThrowsBusinessRuleException()
    {
        var admin = new User { Id = Guid.NewGuid(), FullName = "Admin", Email = "adm2@x.com", Role = UserRole.Admin };
        var customer = new User { Id = Guid.NewGuid(), FullName = "Cust", Email = "c2@x.com", Role = UserRole.Customer };
        var notAnAgent = new User { Id = Guid.NewGuid(), FullName = "Not An Agent", Email = "n1@x.com", Role = UserRole.Customer, IsActive = true };
        var ticket = new Ticket { Id = Guid.NewGuid(), TicketNumber = "TKT-000002", Title = "t", Description = "d", CustomerId = customer.Id, Status = TicketStatus.Open };

        _context.Users.AddRange(admin, customer, notAnAgent);
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();

        var handler = CreateHandler(admin.Id);

        var act = () => handler.Handle(new AssignTicketCommand { TicketId = ticket.Id, AgentId = notAnAgent.Id }, default);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Assign_TicketThatDoesNotExist_ThrowsNotFoundException()
    {
        var admin = new User { Id = Guid.NewGuid(), FullName = "Admin", Email = "adm3@x.com", Role = UserRole.Admin };
        var agent = new User { Id = Guid.NewGuid(), FullName = "Agent", Email = "a2@x.com", Role = UserRole.SupportAgent, IsActive = true };
        _context.Users.AddRange(admin, agent);
        await _context.SaveChangesAsync();

        var handler = CreateHandler(admin.Id);

        var act = () => handler.Handle(new AssignTicketCommand { TicketId = Guid.NewGuid(), AgentId = agent.Id }, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose() => _context.Dispose();
}
