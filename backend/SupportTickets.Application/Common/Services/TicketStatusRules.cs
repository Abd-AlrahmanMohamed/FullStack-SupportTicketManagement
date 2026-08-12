using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Common.Services;

/// <summary>
/// Centralized ticket status state machine. Closing a ticket is handled by a
/// dedicated use case (CloseTicket) and is intentionally not part of this matrix -
/// a ticket can only become Closed from Resolved, via that separate, more tightly
/// authorized command.
/// </summary>
public static class TicketStatusRules
{
    private static readonly Dictionary<TicketStatus, TicketStatus[]> AllowedTransitions = new()
    {
        [TicketStatus.Open] = new[] { TicketStatus.InProgress },
        [TicketStatus.InProgress] = new[] { TicketStatus.Resolved, TicketStatus.Open },
        [TicketStatus.Resolved] = new[] { TicketStatus.InProgress },
        [TicketStatus.Closed] = Array.Empty<TicketStatus>()
    };

    public static bool IsValidTransition(TicketStatus from, TicketStatus to)
    {
        return AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);
    }

    public static bool CanClose(TicketStatus current) => current == TicketStatus.Resolved;
}
