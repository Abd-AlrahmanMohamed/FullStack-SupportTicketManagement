using SupportTickets.Domain.Common;

namespace SupportTickets.Domain.Entities;

public class TimeEntry : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid AgentId { get; set; }
    public User Agent { get; set; } = null!;

    public DateTime WorkDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
}
