using MediatR;
using Microsoft.EntityFrameworkCore;
using SupportTickets.Application.Common.Dtos;
using SupportTickets.Application.Common.Interfaces;
using SupportTickets.Domain.Entities;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Application.Features.Dashboard.Queries.GetDashboard;

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IGenericRepository<Ticket> _ticketRepository;

    public GetDashboardQueryHandler(IGenericRepository<Ticket> ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken cancellationToken)
    {
        var tickets = _ticketRepository.Query().AsNoTracking();

        var totalTickets = await tickets.CountAsync(cancellationToken);
        var openTickets = await tickets.CountAsync(t => t.Status == TicketStatus.Open, cancellationToken);
        var inProgressTickets = await tickets.CountAsync(t => t.Status == TicketStatus.InProgress, cancellationToken);
        var resolvedTickets = await tickets.CountAsync(t => t.Status == TicketStatus.Resolved, cancellationToken);
        var closedTickets = await tickets.CountAsync(t => t.Status == TicketStatus.Closed, cancellationToken);
        var openCriticalTickets = await tickets.CountAsync(
            t => t.Status == TicketStatus.Open && t.Priority == TicketPriority.Critical, cancellationToken);

        // Only the two timestamps needed for the calculation are pulled back, not full ticket rows.
        var resolutionTimestamps = await tickets
            .Where(t => t.ResolvedAt != null)
            .Select(t => new { t.CreatedAt, t.ResolvedAt })
            .ToListAsync(cancellationToken);

        var averageResolutionTimeHours = resolutionTimestamps.Count == 0
            ? 0
            : Math.Round(resolutionTimestamps.Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours), 2);

        var agentWorkload = await tickets
            .Where(t => t.AssignedAgentId != null &&
                        (t.Status == TicketStatus.Open || t.Status == TicketStatus.InProgress))
            .GroupBy(t => new { t.AssignedAgentId, t.AssignedAgent!.FullName })
            .Select(g => new AgentWorkloadDto
            {
                AgentName = g.Key.FullName,
                ActiveTickets = g.Count()
            })
            .OrderByDescending(g => g.ActiveTickets)
            .ToListAsync(cancellationToken);

        return new DashboardDto
        {
            TotalTickets = totalTickets,
            OpenTickets = openTickets,
            InProgressTickets = inProgressTickets,
            ResolvedTickets = resolvedTickets,
            ClosedTickets = closedTickets,
            OpenCriticalTickets = openCriticalTickets,
            AverageResolutionTimeHours = averageResolutionTimeHours,
            AgentWorkload = agentWorkload
        };
    }
}
