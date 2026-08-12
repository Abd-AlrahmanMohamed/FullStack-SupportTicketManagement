using SupportTickets.Application.Common.Exceptions;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Common.Services;

/// <summary>
/// Central customer/agent data-isolation check. A customer may only reach their own
/// tickets, an agent only tickets assigned to them, an admin reaches everything.
/// Denied access surfaces as NotFound rather than Forbidden so an unauthorized caller
/// cannot distinguish "not yours" from "does not exist".
/// </summary>
public static class TicketAccessGuard
{
    public static void EnsureCanAccess(Ticket ticket, Guid? currentUserId, UserRole? currentRole)
    {
        if (CanAccess(ticket, currentUserId, currentRole))
        {
            return;
        }

        throw new NotFoundException(nameof(Ticket), ticket.Id);
    }

    public static bool CanAccess(Ticket ticket, Guid? currentUserId, UserRole? currentRole)
    {
        return currentRole switch
        {
            UserRole.Admin => true,
            UserRole.Customer => ticket.CustomerId == currentUserId,
            UserRole.SupportAgent => ticket.AssignedAgentId == currentUserId,
            _ => false
        };
    }

    public static IQueryable<Ticket> ScopeToCurrentUser(IQueryable<Ticket> query, Guid? currentUserId, UserRole? currentRole)
    {
        return currentRole switch
        {
            UserRole.Admin => query,
            UserRole.Customer => query.Where(t => t.CustomerId == currentUserId),
            UserRole.SupportAgent => query.Where(t => t.AssignedAgentId == currentUserId),
            _ => query.Where(_ => false)
        };
    }
}
