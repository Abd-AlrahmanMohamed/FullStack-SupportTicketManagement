using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Features.Tickets.Commands.CloseTicket;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using SupportTickets.Infrastructure.Persistence;
using SupportTickets.Infrastructure.Repositories;
using SupportTickets.Tests.TestUtilities;
using Xunit;

namespace SupportTickets.Tests.Unit.Tickets;

public class CloseTicketCommandHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _context = TestDbContextFactory.Create();

    private CloseTicketCommandHandler CreateHandler(Guid actingUserId, UserRole actingRole)
    {
        var currentUser = new FakeCurrentUserService { UserId = actingUserId, Role = actingRole };

        return new CloseTicketCommandHandler(
            new GenericRepository<Ticket>(_context),
            new GenericRepository<TicketActivity>(_context),
            new UnitOfWork(_context),
            currentUser,
            MapperFactory.Create(),
            NullLogger<CloseTicketCommandHandler>.Instance);
    }

    private async Task<(User customer, Ticket ticket)> SeedTicketAsync(TicketStatus status)
    {
        var customer = new User { Id = Guid.NewGuid(), FullName = "Cust", Email = $"{Guid.NewGuid()}@x.com", Role = UserRole.Customer };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            TicketNumber = $"TKT-{Guid.NewGuid():N}"[..10],
            Title = "t",
            Description = "d",
            CustomerId = customer.Id,
            Status = status
        };

        _context.Users.Add(customer);
        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return (customer, ticket);
    }

    [Fact]
    public async Task Customer_CannotClose_UnresolvedTicket()
    {
        var (customer, ticket) = await SeedTicketAsync(TicketStatus.Open);
        var handler = CreateHandler(customer.Id, UserRole.Customer);

        var act = () => handler.Handle(new CloseTicketCommand { TicketId = ticket.Id }, default);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Customer_CanClose_ResolvedTicket()
    {
        var (customer, ticket) = await SeedTicketAsync(TicketStatus.Resolved);
        var handler = CreateHandler(customer.Id, UserRole.Customer);

        var result = await handler.Handle(new CloseTicketCommand { TicketId = ticket.Id }, default);

        result.Status.Should().Be("Closed");
    }

    [Fact]
    public async Task Customer_CannotClose_AnotherCustomersTicket()
    {
        var (_, ticket) = await SeedTicketAsync(TicketStatus.Resolved);
        var otherCustomerId = Guid.NewGuid();
        var handler = CreateHandler(otherCustomerId, UserRole.Customer);

        var act = () => handler.Handle(new CloseTicketCommand { TicketId = ticket.Id }, default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    public void Dispose() => _context.Dispose();
}
