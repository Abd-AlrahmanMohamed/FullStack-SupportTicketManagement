using FluentAssertions;
using SupportTickets.Application.Common.Services;
using SupportTickets.Domain.Enums;
using Xunit;

namespace SupportTickets.Tests.Unit.Common;

public class TicketStatusRulesTests
{
    [Theory]
    [InlineData(TicketStatus.Open, TicketStatus.InProgress, true)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Resolved, true)]
    [InlineData(TicketStatus.InProgress, TicketStatus.Open, true)]
    [InlineData(TicketStatus.Resolved, TicketStatus.InProgress, true)]
    [InlineData(TicketStatus.Open, TicketStatus.Closed, false)]
    [InlineData(TicketStatus.Open, TicketStatus.Resolved, false)]
    [InlineData(TicketStatus.Resolved, TicketStatus.Closed, false)]
    [InlineData(TicketStatus.Closed, TicketStatus.Open, false)]
    [InlineData(TicketStatus.Closed, TicketStatus.InProgress, false)]
    public void IsValidTransition_MatchesExpectedStateMachine(TicketStatus from, TicketStatus to, bool expected)
    {
        TicketStatusRules.IsValidTransition(from, to).Should().Be(expected);
    }

    [Fact]
    public void CanClose_ReturnsTrue_WhenResolved()
    {
        TicketStatusRules.CanClose(TicketStatus.Resolved).Should().BeTrue();
    }

    [Theory]
    [InlineData(TicketStatus.Open)]
    [InlineData(TicketStatus.InProgress)]
    [InlineData(TicketStatus.Closed)]
    public void CanClose_ReturnsFalse_WhenNotResolved(TicketStatus status)
    {
        TicketStatusRules.CanClose(status).Should().BeFalse();
    }
}
