using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Features.Tickets.Commands.UpdateTicketStatus;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Infrastructure.Persistence;
using SupportTickets.Infrastructure.Repositories;
using SupportTickets.Tests.TestUtilities;
using Xunit;

namespace SupportTickets.Tests.Unit.Tickets;

public class UpdateTicketStatusCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context = TestDbContextFactory.Create();

    private UpdateTicketStatusCommandHandler CreateHandler(Guid actingUserId, UserRole actingRole)
    {
        var currentUser = new FakeCurrentUserService { UserId = actingUserId, Role = actingRole };

        return new UpdateTicketStatusCommandHandler(
            new GenericRepository<Ticket>(_context),
            new GenericRepository<TicketActivity>(_context),
            new UnitOfWork(_context),
            currentUser,
            MapperFactory.Create(),
            NullLogger<UpdateTicketStatusCommandHandler>.Instance);
    }

    private async Task<User> SeedUserAsync(UserRole role)
    {
        var user = new User { Id = Guid.NewGuid(), FullName = role.ToString(), Email = $"{Guid.NewGuid()}@x.com", Role = role, IsActive = true };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    private async Task<Ticket> SeedTicketAsync(TicketStatus status, Guid customerId, Guid? assignedAgentId = null)
    {
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = $"TKT-{Guid.NewGuid():N}"[..10],
            Title = "t",
            Description = "d",
            CustomerId = customerId,
            Status = status,
            AssignedAgentId = assignedAgentId
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    [Fact]
    public async Task Admin_CanMove_OpenToInProgress()
    {
        var admin = await SeedUserAsync(UserRole.Admin);
        var customer = await SeedUserAsync(UserRole.Customer);
        var ticket = await SeedTicketAsync(TicketStatus.Open, customer.Id);

        var handler = CreateHandler(admin.Id, UserRole.Admin);
        var result = await handler.Handle(new UpdateTicketStatusCommand { TicketId = ticket.Id, Status = "InProgress" }, default);

        result.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task InvalidTransition_OpenToClosed_ThrowsBusinessRuleException()
    {
        var admin = await SeedUserAsync(UserRole.Admin);
        var customer = await SeedUserAsync(UserRole.Customer);
        var ticket = await SeedTicketAsync(TicketStatus.Open, customer.Id);

        var handler = CreateHandler(admin.Id, UserRole.Admin);
        var act = () => handler.Handle(new UpdateTicketStatusCommand { TicketId = ticket.Id, Status = "Closed" }, default);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task SupportAgent_NotAssignedToTicket_CannotUpdateStatus()
    {
        var assignedAgent = await SeedUserAsync(UserRole.SupportAgent);
        var otherAgent = await SeedUserAsync(UserRole.SupportAgent);
        var customer = await SeedUserAsync(UserRole.Customer);
        var ticket = await SeedTicketAsync(TicketStatus.Open, customer.Id, assignedAgentId: assignedAgent.Id);

        var handler = CreateHandler(otherAgent.Id, UserRole.SupportAgent);
        var act = () => handler.Handle(new UpdateTicketStatusCommand { TicketId = ticket.Id, Status = "InProgress" }, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SupportAgent_AssignedToTicket_CanUpdateStatus()
    {
        var agent = await SeedUserAsync(UserRole.SupportAgent);
        var customer = await SeedUserAsync(UserRole.Customer);
        var ticket = await SeedTicketAsync(TicketStatus.Open, customer.Id, assignedAgentId: agent.Id);

        var handler = CreateHandler(agent.Id, UserRole.SupportAgent);
        var result = await handler.Handle(new UpdateTicketStatusCommand { TicketId = ticket.Id, Status = "InProgress" }, default);

        result.Status.Should().Be("InProgress");
    }

    public void Dispose() => _context.Dispose();
}
