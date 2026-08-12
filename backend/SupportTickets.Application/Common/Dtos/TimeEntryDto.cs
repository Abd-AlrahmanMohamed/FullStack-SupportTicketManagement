namespace SupportTickets.Application.Common.Dtos;

public class TimeEntryDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public DateTime WorkDate { get; set; }
    public int DurationMinutes { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
