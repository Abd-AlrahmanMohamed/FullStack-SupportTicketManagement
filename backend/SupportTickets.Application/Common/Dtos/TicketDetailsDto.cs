namespace SupportTickets.Application.Common.Dtos;

public class TicketDetailsDto
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;

    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;

    public Guid? AssignedAgentId { get; set; }
    public string? AssignedAgentName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    public int TotalTimeMinutes { get; set; }

    public List<CommentDto> Comments { get; set; } = new();
    public List<ActivityDto> Timeline { get; set; } = new();
    public List<TimeEntryDto> TimeEntries { get; set; } = new();
}
