using SupportTickets.Domain.Common;
using SupportTickets.Domain.Enums;

namespace SupportTickets.Domain.Entities;

public class Ticket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TicketStatus Status { get; set; } = TicketStatus.Open;
    public TicketPriority Priority { get; set; } = TicketPriority.Medium;

    public Guid CustomerId { get; set; }
    public User Customer { get; set; } = null!;

    public Guid? AssignedAgentId { get; set; }
    public User? AssignedAgent { get; set; }

    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    public ICollection<TicketActivity> Activities { get; set; } = new List<TicketActivity>();
    public ICollection<TimeEntry> TimeEntries { get; set; } = new List<TimeEntry>();
}
