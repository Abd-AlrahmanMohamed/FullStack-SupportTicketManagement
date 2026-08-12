namespace SupportTickets.Application.Common.Dtos;

public class TicketDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }
    public DateTime CreatedAt { get; set; }
}
