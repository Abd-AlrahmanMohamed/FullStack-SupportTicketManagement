using FluentAssertions;
using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Application.Common.Services;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;
using Xunit;

namespace SupportTickets.Tests.Unit.Common;

public class TicketAccessGuardTests
{
    private static Ticket CreateTicket(Guid customerId, Guid? assignedAgentId = null)
    {
        return new Ticket { Id = Guid.NewGuid(), CustomerId = customerId, AssignedAgentId = assignedAgentId };
    }

    [Fact]
    public void Admin_CanAccess_AnyTicket()
    {
        var ticket = CreateTicket(customerId: Guid.NewGuid());

        var act = () => TicketAccessGuard.EnsureCanAccess(ticket, Guid.NewGuid(), UserRole.Admin);

        act.Should().NotThrow();
    }

    [Fact]
    public void Customer_CanAccess_OwnTicket()
    {
        var customerId = Guid.NewGuid();
        var ticket = CreateTicket(customerId);

        var act = () => TicketAccessGuard.EnsureCanAccess(ticket, customerId, UserRole.Customer);

        act.Should().NotThrow();
    }

    [Fact]
    public void Customer_CannotAccess_AnotherCustomersTicket()
    {
        var ownerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();
        var ticket = CreateTicket(ownerId);

        var act = () => TicketAccessGuard.EnsureCanAccess(ticket, otherCustomerId, UserRole.Customer);

        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void SupportAgent_CanAccess_AssignedTicket()
    {
        var agentId = Guid.NewGuid();
        var ticket = CreateTicket(customerId: Guid.NewGuid(), assignedAgentId: agentId);

        var act = () => TicketAccessGuard.EnsureCanAccess(ticket, agentId, UserRole.SupportAgent);

        act.Should().NotThrow();
    }

    [Fact]
    public void SupportAgent_CannotAccess_TicketNotAssignedToThem()
    {
        var assignedAgentId = Guid.NewGuid();
        var otherAgentId = Guid.NewGuid();
        var ticket = CreateTicket(customerId: Guid.NewGuid(), assignedAgentId: assignedAgentId);

        var act = () => TicketAccessGuard.EnsureCanAccess(ticket, otherAgentId, UserRole.SupportAgent);

        act.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void SupportAgent_CannotAccess_UnassignedTicket()
    {
        var ticket = CreateTicket(customerId: Guid.NewGuid(), assignedAgentId: null);

        var act = () => TicketAccessGuard.EnsureCanAccess(ticket, Guid.NewGuid(), UserRole.SupportAgent);

        act.Should().Throw<NotFoundException>();
    }
}
