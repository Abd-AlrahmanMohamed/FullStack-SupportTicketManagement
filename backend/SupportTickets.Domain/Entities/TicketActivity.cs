using SupportTickets.Domain.Common;

namespace SupportTickets.Domain.Entities;

public class TicketActivity : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Action { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
