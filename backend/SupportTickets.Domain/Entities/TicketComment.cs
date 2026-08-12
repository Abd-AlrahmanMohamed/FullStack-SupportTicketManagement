using SupportTickets.Domain.Common;

namespace SupportTickets.Domain.Entities;

public class TicketComment : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
}
